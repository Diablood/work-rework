using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;

internal static class GameIlContracts
{
    internal static void Check(string gameDll)
    {
        using var stream = File.OpenRead(gameDll);
        using var pe = new PEReader(stream);
        MetadataReader metadata = pe.GetMetadataReader();
        var opcodes = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.FieldType == typeof(OpCode)).Select(field => (OpCode)field.GetValue(null)!)
            .ToDictionary(opcode => unchecked((ushort)opcode.Value));

        CheckConstants("RimWorld", "Pawn_WorkSettings", "SetPriority", 1);
        CheckConstants("RimWorld", "WidgetsWork", "DrawWorkBoxFor", 2);
        CheckConstants("RimWorld", "PawnColumnWorker_WorkPriority", "HeaderClicked", 2);
        var scanner = Read("RimWorld", "JobGiver_Work", "TryIssueJobPackage");
        int fields = scanner.Count(instruction => instruction.Op == OpCodes.Ldfld && FieldName(instruction.Operand) == "priorityInType");
        if (fields != 2) throw new Exception($"Scanner IL contract: expected 2 priorityInType reads, found {fields}");
        Console.WriteLine("Game IL contracts: three range boundaries and two scanner field reads verified without loading game code.");

        void CheckConstants(string ns, string type, string method, int expected)
        {
            var body = Read(ns, type, method);
            int count = 0;
            for (int i = 0; i + 1 < body.Count; i++)
            {
                bool four = body[i].Op == OpCodes.Ldc_I4_4 ||
                    (body[i].Op == OpCodes.Ldc_I4 || body[i].Op == OpCodes.Ldc_I4_S) && body[i].Operand == 4;
                OpCode next = body[i + 1].Op;
                if (four && (next.FlowControl == FlowControl.Cond_Branch || next.Name?.StartsWith("stloc", StringComparison.Ordinal) == true)) count++;
            }
            if (count != expected) throw new Exception($"{type}.{method}: expected {expected} priority-4 constants, found {count}");
        }

        string? FieldName(int token)
        {
            EntityHandle handle = MetadataTokens.EntityHandle(token);
            return handle.Kind switch
            {
                HandleKind.FieldDefinition => metadata.GetString(metadata.GetFieldDefinition((FieldDefinitionHandle)handle).Name),
                HandleKind.MemberReference => metadata.GetString(metadata.GetMemberReference((MemberReferenceHandle)handle).Name),
                _ => null
            };
        }

        List<(OpCode Op, int Operand)> Read(string ns, string typeName, string methodName)
        {
            TypeDefinition type = metadata.TypeDefinitions.Select(metadata.GetTypeDefinition).Single(type =>
                metadata.GetString(type.Namespace) == ns && metadata.GetString(type.Name) == typeName);
            MethodDefinition method = type.GetMethods().Select(metadata.GetMethodDefinition)
                .Single(method => metadata.GetString(method.Name) == methodName);
            byte[] il = pe.GetMethodBody(method.RelativeVirtualAddress).GetILBytes() ?? throw new Exception("Missing IL body");
            var result = new List<(OpCode, int)>();
            int offset = 0;
            while (offset < il.Length)
            {
                ushort code = il[offset++];
                if (code == 0xfe) code = (ushort)(0xfe00 | il[offset++]);
                OpCode op = opcodes[code];
                int operand = 0;
                int bytes = op.OperandType switch
                {
                    OperandType.InlineNone => 0,
                    OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar => 1,
                    OperandType.InlineVar => 2,
                    OperandType.InlineI8 or OperandType.InlineR => 8,
                    OperandType.InlineSwitch => 4 + BitConverter.ToInt32(il, offset) * 4,
                    _ => 4
                };
                if (op.OperandType == OperandType.ShortInlineI) operand = unchecked((sbyte)il[offset]);
                else if (bytes == 4) operand = BitConverter.ToInt32(il, offset);
                offset += bytes;
                if (offset > il.Length) throw new Exception("Invalid IL instruction boundary");
                result.Add((op, operand));
            }
            return result;
        }
    }
}

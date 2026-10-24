using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;

// Inspect the optional net48 assembly without loading it or resolving its old
// Unity references into the .NET 10 game process.
internal static class GroupedContracts
{
    internal static void Check(string path)
    {
        using var stream = File.OpenRead(path);
        using var pe = new PEReader(stream);
        MetadataReader reader = pe.GetMetadataReader();
        var provider = new Names();
        int checkedMembers = 0;
        Method("GroupColumnDefResolver", "GetResolver", "PawnTableGrouped.GroupColumnWorkerDef", "RimWorld.PawnColumnDef", "System.Boolean");
        Method("GroupColumnWorker", "get_ColumnDef", "RimWorld.PawnColumnDef");
        Method("GroupColumnWorker_WorkPriority", "GetValue", "System.Object", "Verse.Pawn");
        Method("GroupColumnWorker_WorkPriority", "SetValue", "System.Void", "Verse.Pawn", "System.Object", "RimWorld.PawnTable");
        Method("GroupColumnWorker_WorkPriority", "IsVisible", "System.Boolean", "Verse.Pawn");
        Method("GroupColumnWorker_WorkBase", "DoCell", "System.Void", "UnityEngine.Rect", "PawnTableGrouped.PawnTableGroupColumn", "RimWorld.PawnTable");
        Method("PawnTableGroupedModel", "ReadTableConfig", "System.Void");
        Method("PawnTableExtensions", "TryGetImplementation", "System.Boolean", "RimWorld.PawnTable", "PawnTableGrouped.PawnTableGroupedImpl&");
        Method("GroupColumnWorkerDef", "get_Worker", "PawnTableGrouped.GroupColumnWorker");
        Method("PawnTableGroupColumn", "get_Group", "PawnTableGrouped.PawnTableGroup");
        Method("PawnTableGroupColumn", "NotifyValueChanged", "System.Void");
        Method("PawnTableGroupedModel", "get_Table", "RimWorld.PawnTable");
        Field("GroupColumnWorkerDef", "workerClass", "System.Type");
        Field("GroupColumnWorkerDef", "workerInt", "PawnTableGrouped.GroupColumnWorker");
        Field("PawnTableGroup", "Pawns", "System.Collections.Generic.List`1<Verse.Pawn>");
        Field("PawnTableGroupedModel", "AllowHScroll", "System.Boolean");
        Method("CPawnTableGroupRow", "DoCell", "System.Void", "UnityEngine.Rect", "System.Int32", "System.Boolean", "System.Boolean");
        Method("PawnTableGroup", "get_Table", "RimWorld.PawnTable");
        Field("CPawnTableGroupRow", "group", "PawnTableGrouped.PawnTableGroup");
        Field("Settings", "showDummyColumns", "System.Boolean");
        Method("CTableGrid", "DoScrollbars", "System.Void");
        Method("CTableGrid", "get_DataSource", "PawnTableGrouped.TableGrid.ICTableGridDataSource");
        Field("PawnTableGroupedView", "model", "PawnTableGrouped.PawnTableGroupedModel");
        Method("CTableGrid", "PostLayoutUpdate", "System.Void");
        foreach (string name in new[] { "hScrollVisible", "vScrollVisible", "AllowHScroll" }) Field("CTableGrid", name, "System.Boolean");
        foreach (string name in new[] { "fixedWidth", "fixedHeight", "scrollInnerWidth", "scrollInnerHeight", "scrollOuterWidth", "scrollOuterHeight", "hScrollMax", "vScrollMax" })
            Field("CTableGrid", name, "System.Single");
        foreach (string name in new[] { "scrollOuterRectClipped", "hScrollRect", "vScrollRect", "fixedPanelRect", "columnsPanelRect", "tableBodyOuterRect", "namesPanelRect" })
            Field("CTableGrid", name, "UnityEngine.Rect");
        Field("CTableGrid", "scrollPosition", "UnityEngine.Vector2");
        var opcodes = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static).Where(f => f.FieldType == typeof(OpCode))
            .Select(f => (OpCode)f.GetValue(null)!).ToDictionary(o => unchecked((ushort)o.Value));
        if (Body("CPawnTableGroupRow", "DoCell").Count(i => i.Op == OpCodes.Ldfld && MemberName(i.Token) == "showDummyColumns") != 1)
            throw new Exception("Grouped diagnostic flag IL contract changed.");
        if (Body("CTableGrid", "DoScrollbars").Count(i => i.Op == OpCodes.Call && MemberName(i.Token) == "HorizontalScrollbar") != 1)
            throw new Exception("Grouped horizontal scrollbar IL contract changed.");
        Console.WriteLine($"Grouped Pawns Lists contracts: {checkedMembers} members and two IL sites verified, ten optional hooks covered; no game or mod code executed.");

        TypeDefinition Type(string name) => reader.TypeDefinitions.Select(reader.GetTypeDefinition).Single(t => reader.GetString(t.Namespace) == "PawnTableGrouped" && reader.GetString(t.Name) == name);
        void Method(string type, string name, string result, params string[] parameters)
        {
            MethodDefinition method = Type(type).GetMethods().Select(reader.GetMethodDefinition).Single(m => reader.GetString(m.Name) == name);
            MethodSignature<string> signature = method.DecodeSignature(provider, (object?)null);
            if (signature.ReturnType != result || !signature.ParameterTypes.SequenceEqual(parameters))
                throw new Exception($"Grouped signature changed: {type}.{name}: {signature.ReturnType} ({string.Join(",", signature.ParameterTypes)})");
            checkedMembers++;
        }
        void Field(string type, string name, string expected)
        {
            FieldDefinition field = Type(type).GetFields().Select(reader.GetFieldDefinition).Single(f => reader.GetString(f.Name) == name);
            if (field.DecodeSignature(provider, (object?)null) != expected) throw new Exception($"Grouped field changed: {type}.{name}");
            checkedMembers++;
        }

        string? MemberName(int token)
        {
            EntityHandle handle = MetadataTokens.EntityHandle(token);
            return handle.Kind switch
            {
                HandleKind.FieldDefinition => reader.GetString(reader.GetFieldDefinition((FieldDefinitionHandle)handle).Name),
                HandleKind.MethodDefinition => reader.GetString(reader.GetMethodDefinition((MethodDefinitionHandle)handle).Name),
                HandleKind.MemberReference => reader.GetString(reader.GetMemberReference((MemberReferenceHandle)handle).Name),
                _ => null
            };
        }
        IEnumerable<(OpCode Op, int Token)> Body(string type, string name)
        {
            MethodDefinition method = Type(type).GetMethods().Select(reader.GetMethodDefinition).Single(m => reader.GetString(m.Name) == name);
            byte[] bytes = pe.GetMethodBody(method.RelativeVirtualAddress).GetILBytes() ?? throw new Exception("Missing grouped IL body");
            int offset = 0;
            while (offset < bytes.Length)
            {
                ushort code = bytes[offset++];
                if (code == 0xfe) code = (ushort)(0xfe00 | bytes[offset++]);
                OpCode op = opcodes[code];
                int size = op.OperandType switch
                {
                    OperandType.InlineNone => 0,
                    OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar => 1,
                    OperandType.InlineVar => 2,
                    OperandType.InlineI8 or OperandType.InlineR => 8,
                    OperandType.InlineSwitch => 4 + BitConverter.ToInt32(bytes, offset) * 4,
                    _ => 4
                };
                int token = op.OperandType is OperandType.InlineField or OperandType.InlineMethod ? BitConverter.ToInt32(bytes, offset) : 0;
                offset += size;
                if (offset > bytes.Length) throw new Exception("Invalid grouped IL boundary");
                yield return (op, token);
            }
        }
    }

    private sealed class Names : ISignatureTypeProvider<string, object?>
    {
        public string GetArrayType(string type, ArrayShape shape) => type + "[]";
        public string GetByReferenceType(string type) => type + "&";
        public string GetFunctionPointerType(MethodSignature<string> signature) => "function";
        public string GetGenericInstantiation(string type, ImmutableArray<string> args) => type + "<" + string.Join(",", args) + ">";
        public string GetGenericMethodParameter(object? context, int index) => "!!" + index;
        public string GetGenericTypeParameter(object? context, int index) => "!" + index;
        public string GetModifiedType(string modifier, string type, bool required) => type;
        public string GetPinnedType(string type) => type;
        public string GetPointerType(string type) => type + "*";
        public string GetPrimitiveType(PrimitiveTypeCode code) => code switch { PrimitiveTypeCode.Void => "System.Void", PrimitiveTypeCode.Boolean => "System.Boolean", PrimitiveTypeCode.Int32 => "System.Int32", PrimitiveTypeCode.Object => "System.Object", _ => "System." + code };
        public string GetSZArrayType(string type) => type + "[]";
        public string GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawKind)
        { var type = reader.GetTypeDefinition(handle); return reader.GetString(type.Namespace) + "." + reader.GetString(type.Name); }
        public string GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawKind)
        { var type = reader.GetTypeReference(handle); return reader.GetString(type.Namespace) + "." + reader.GetString(type.Name); }
        public string GetTypeFromSpecification(MetadataReader reader, object? context, TypeSpecificationHandle handle, byte rawKind)
            => reader.GetTypeSpecification(handle).DecodeSignature(this, context);
    }
}

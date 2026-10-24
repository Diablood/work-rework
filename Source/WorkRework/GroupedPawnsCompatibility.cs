using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace WorkRework;

// Optional bridge: no compile-time reference to Grouped Pawns Lists or RWLayout.
internal static class GroupedPawnsCompatibility
{
    internal const string PackageId = "name.krypt.rimworld.pawntablegrouped";
    private sealed record Binding(PawnColumnDef Column);
    private static readonly ConditionalWeakTable<object, Binding> Columns = new();
    private static Type workWorker = null!;
    private static PropertyInfo workerProperty = null!;
    private static PropertyInfo columnProperty = null!;
    private static PropertyInfo groupProperty = null!;
    private static PropertyInfo modelTable = null!;
    private static PropertyInfo groupTable = null!;
    private static FieldInfo rowGroup = null!;
    private static FieldInfo diagnosticFlag = null!;
    private static PropertyInfo gridSource = null!;
    private static FieldInfo viewModel = null!;
    private static PropertyInfo gridBounds = null!;
    private static Dictionary<string, FieldInfo> gridFields = null!;
    private sealed class ScrollDrag { internal float Offset; internal int Control; }
    private static readonly ConditionalWeakTable<object, ScrollDrag> ScrollDrags = new();
    private static FieldInfo workerClass = null!;
    private static FieldInfo workerCache = null!;
    private static FieldInfo groupPawns = null!;
    private static FieldInfo allowScroll = null!;
    private static MethodInfo notifyValues = null!;
    private static Func<PawnTable, bool>? isGrouped;
    internal static bool Active { get; private set; }
    internal static bool Handles(PawnTable table) => Active && isGrouped!(table);

    internal static void Initialize()
    {
        if (!LoadedModManager.RunningModsListForReading.Any(m => m.PackageIdPlayerFacing.Equals(PackageId, StringComparison.OrdinalIgnoreCase) ||
            m.PackageIdPlayerFacing.Equals(PackageId + "_steam", StringComparison.OrdinalIgnoreCase))) return;
        var harmony = new Harmony(WorkReworkMod.PackageId + ".grouped");
        try
        {
            Type worker = RequireType("GroupColumnWorker");
            Type workerDef = RequireType("GroupColumnWorkerDef");
            Type groupColumn = RequireType("PawnTableGroupColumn");
            Type group = RequireType("PawnTableGroup");
            Type model = RequireType("PawnTableGroupedModel");
            workWorker = RequireType("GroupColumnWorker_WorkPriority");
            workerProperty = RequireProperty(workerDef, "Worker");
            columnProperty = RequireProperty(worker, "ColumnDef");
            groupProperty = RequireProperty(groupColumn, "Group");
            modelTable = RequireProperty(model, "Table");
            groupTable = RequireProperty(group, "Table");
            rowGroup = RequireField(RequireType("CPawnTableGroupRow"), "group");
            diagnosticFlag = RequireField(RequireType("Settings"), "showDummyColumns");
            gridSource = RequireProperty(RequireType("CTableGrid"), "DataSource");
            Type gridType = RequireType("CTableGrid");
            gridBounds = RequireProperty(gridType, "BoundsRounded");
            gridFields = new[] { "hScrollVisible", "vScrollVisible", "fixedWidth", "fixedHeight", "scrollInnerWidth", "scrollInnerHeight",
                "scrollOuterWidth", "scrollOuterHeight", "scrollOuterRectClipped", "hScrollMax", "vScrollMax", "scrollPosition",
                "hScrollRect", "vScrollRect", "fixedPanelRect", "columnsPanelRect", "tableBodyOuterRect", "namesPanelRect", "AllowHScroll" }
                .ToDictionary(name => name, name => RequireField(gridType, name));
            viewModel = RequireField(RequireType("PawnTableGroupedView"), "model");
            workerClass = RequireField(workerDef, "workerClass");
            workerCache = RequireField(workerDef, "workerInt");
            groupPawns = RequireField(group, "Pawns");
            allowScroll = RequireField(model, "AllowHScroll");
            notifyValues = RequireMethod(groupColumn, "NotifyValueChanged");
            MethodInfo tryImplementation = RequireMethod(RequireType("PawnTableExtensions"), "TryGetImplementation");
            ParameterExpression table = Expression.Parameter(typeof(PawnTable), "table");
            ParameterExpression implementation = Expression.Variable(tryImplementation.GetParameters()[1].ParameterType.GetElementType()!);
            isGrouped = Expression.Lambda<Func<PawnTable, bool>>(Expression.Block(new[] { implementation }, Expression.Call(tryImplementation, table, implementation)), table).Compile();

            // Resolve every target before installing any optional patches.
            var patches = new (MethodInfo Target, string Handler, bool Prefix)[]
            {
                (RequireMethod(RequireType("GroupColumnDefResolver"), "GetResolver"), nameof(ResolverPostfix), false),
                (columnProperty.GetMethod!, nameof(ColumnPrefix), true),
                (RequireMethod(workWorker, "GetValue"), nameof(ValuePrefix), true),
                (RequireMethod(workWorker, "SetValue"), nameof(SetPrefix), true),
                (RequireMethod(workWorker, "IsVisible"), nameof(VisiblePrefix), true),
                (RequireMethod(RequireType("GroupColumnWorker_WorkBase"), "DoCell"), nameof(CellPrefix), true),
                (RequireMethod(model, "ReadTableConfig"), nameof(ModelPostfix), false)
            };
            MethodInfo groupRowCell = RequireMethod(RequireType("CPawnTableGroupRow"), "DoCell");
            MethodInfo scrollbars = RequireMethod(RequireType("CTableGrid"), "DoScrollbars");
            MethodInfo gridLayout = RequireMethod(gridType, "PostLayoutUpdate");
            foreach (var patch in patches)
            {
                var handler = new HarmonyMethod(typeof(GroupedPawnsCompatibility), patch.Handler);
                harmony.Patch(patch.Target, prefix: patch.Prefix ? handler : null, postfix: patch.Prefix ? null : handler);
            }
            harmony.Patch(groupRowCell, transpiler: new HarmonyMethod(typeof(GroupedPawnsCompatibility), nameof(DiagnosticTranspiler)));
            harmony.Patch(scrollbars, transpiler: new HarmonyMethod(typeof(GroupedPawnsCompatibility), nameof(ScrollbarTranspiler)));
            harmony.Patch(gridLayout, postfix: new HarmonyMethod(typeof(GroupedPawnsCompatibility), nameof(GridLayoutPostfix)));
            Active = true;
            Log.Message("[Work Rework] Grouped Pawns Lists bridge enabled (optional, ten hooks).");
        }
        catch (Exception error)
        {
            harmony.UnpatchAll(WorkReworkMod.PackageId + ".grouped");
            Active = false;
            Log.Warning($"[Work Rework] Grouped Pawns Lists bridge unavailable: {error}");
        }
    }

    private static Type RequireType(string name) => AccessTools.TypeByName("PawnTableGrouped." + name) ?? throw new MissingMemberException(name);
    private static MethodInfo RequireMethod(Type type, string name) => AccessTools.Method(type, name) ?? throw new MissingMethodException(type.FullName, name);
    private static FieldInfo RequireField(Type type, string name) => AccessTools.Field(type, name) ?? throw new MissingFieldException(type.FullName, name);
    private static PropertyInfo RequireProperty(Type type, string name) => AccessTools.Property(type, name) ?? throw new MissingMemberException(type.FullName, name);

    private static IEnumerable<CodeInstruction> DiagnosticTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var result = new List<CodeInstruction>();
        int count = 0;
        foreach (CodeInstruction original in instructions)
        {
            result.Add(new CodeInstruction(original));
            if (!original.LoadsField(diagnosticFlag)) continue;
            result.Add(new CodeInstruction(OpCodes.Ldarg_0));
            result.Add(new CodeInstruction(OpCodes.Ldarg_2));
            result.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GroupedPawnsCompatibility), nameof(ShowDiagnostic))));
            count++;
        }
        if (count != 1) throw new InvalidOperationException($"Grouped diagnostic patch expected one flag read, found {count}.");
        return result;
    }

    private static bool ShowDiagnostic(bool enabled, object row, int columnIndex)
    {
        if (!enabled) return false;
        object? group = rowGroup.GetValue(row);
        if (group == null || groupTable.GetValue(group) is not PawnTable table || !WorkTableAccess.IsWork(table)) return true;
        List<PawnColumnDef> columns = table.Columns;
        if (columnIndex < 0 || columnIndex >= columns.Count) return true;
        PawnColumnDef column = columns[columnIndex];
        // Suppress only the technical duplicate for work cells. Keep the mod's
        // diagnostic preference and all other tables/columns untouched.
        return column is not WorkGiverColumnDef && column.Worker is not PawnColumnWorker_WorkPriority;
    }

    private static IEnumerable<CodeInstruction> ScrollbarTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var result = new List<CodeInstruction>();
        int count = 0;
        foreach (CodeInstruction original in instructions)
        {
            if (original.operand is MethodInfo method && method.DeclaringType?.FullName == "UnityEngine.GUI" && method.Name == "HorizontalScrollbar" && method.GetParameters().Length == 5)
            {
                var grid = new CodeInstruction(OpCodes.Ldarg_0);
                grid.labels.AddRange(original.labels);
                grid.blocks.AddRange(original.blocks);
                result.Add(grid);
                result.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GroupedPawnsCompatibility), nameof(HorizontalScrollbar))));
                count++;
            }
            else result.Add(new CodeInstruction(original));
        }
        if (count != 1) throw new InvalidOperationException($"Grouped scrollbar patch expected one horizontal scrollbar, found {count}.");
        return result;
    }

    private static float HorizontalScrollbar(Rect rect, float value, float size, float start, float end, object grid)
    {
        if (!IsWorkGrid(grid))
            return GUI.HorizontalScrollbar(rect, value, size, start, end);

        float maximum = Mathf.Max(0f, end - start - size);
        value = Mathf.Clamp(value, start, start + maximum);
        float width = Mathf.Clamp(rect.width * size / Mathf.Max(1f, Mathf.Max(size, end - start)), Mathf.Min(24f, rect.width), rect.width);
        float travel = Mathf.Max(0f, rect.width - width);
        float x = rect.x + (maximum > 0f ? (value - start) / maximum * travel : 0f);
        Rect thumb = new(x, rect.center.y - 4f, width, 8f);
        int control = GUIUtility.GetControlID(FocusType.Passive, rect);
        ScrollDrag drag = ScrollDrags.GetValue(grid, _ => new ScrollDrag());
        Event current = Event.current;
        if (current.type == EventType.mouseDown && current.button == 0 && Mouse.IsOver(rect))
        {
            drag.Offset = thumb.Contains(current.mousePosition) ? current.mousePosition.x - x : width / 2f;
            GUIUtility.hotControl = control;
            drag.Control = control;
            value = start + (travel > 0f ? Mathf.Clamp01((current.mousePosition.x - rect.x - drag.Offset) / travel) * maximum : 0f);
            current.Use();
        }
        else if (GUIUtility.hotControl == control && current.type == EventType.mouseDrag)
        {
            value = start + (travel > 0f ? Mathf.Clamp01((current.mousePosition.x - rect.x - drag.Offset) / travel) * maximum : 0f);
            current.Use();
        }
        else if (GUIUtility.hotControl == control && current.rawType == EventType.mouseUp)
        {
            GUIUtility.hotControl = 0;
            drag.Control = 0;
            current.Use();
        }
        thumb.x = rect.x + (maximum > 0f ? (value - start) / maximum * travel : 0f);
        Widgets.DrawBoxSolid(new Rect(rect.x, rect.center.y - 2f, rect.width, 4f), new Color(0.12f, 0.14f, 0.15f, 0.95f));
        Widgets.DrawBoxSolidWithOutline(thumb, Mouse.IsOver(rect) || GUIUtility.hotControl == control
            ? new Color(0.55f, 0.62f, 0.65f) : new Color(0.35f, 0.41f, 0.44f), new Color(0.65f, 0.7f, 0.72f, 0.65f));
        return value;
    }

    private static bool IsWorkGrid(object grid)
    {
        object? source = gridSource.GetValue(grid);
        return source != null && viewModel.DeclaringType!.IsInstanceOfType(source) && viewModel.GetValue(source) is { } model &&
            modelTable.GetValue(model) is PawnTable table && WorkTableAccess.IsWork(table);
    }

    private static void GridLayoutPostfix(object __instance)
    {
        if (!IsWorkGrid(__instance)) return;
        Rect bounds = (Rect)gridBounds.GetValue(__instance)!;
        float Read(string name) => (float)gridFields[name].GetValue(__instance)!;
        void Set(string name, object value) => gridFields[name].SetValue(__instance, value);
        float fixedWidth = Read("fixedWidth");
        float fixedHeight = Read("fixedHeight");
        float contentWidth = Read("scrollInnerWidth") + fixedWidth;
        float contentHeight = Read("scrollInnerHeight") + fixedHeight;
        float verticalWidth = GUI.skin.verticalScrollbar.fixedWidth + GUI.skin.verticalScrollbar.margin.left;
        float horizontalHeight = GUI.skin.horizontalScrollbar.fixedHeight + GUI.skin.horizontalScrollbar.margin.top;
        ScrollViewport viewport = ScrollViewport.Calculate(contentWidth, contentHeight, bounds.width, bounds.height,
            horizontalHeight, verticalWidth, (bool)gridFields["AllowHScroll"].GetValue(__instance)!);
        float outerWidth = Mathf.Max(0f, viewport.Width - fixedWidth);
        float outerHeight = Mathf.Max(0f, viewport.Height - fixedHeight);
        float maximumX = viewport.Horizontal ? Mathf.Max(0f, contentWidth - viewport.Width) : 0f;
        float maximumY = viewport.Vertical ? Mathf.Max(0f, contentHeight - viewport.Height) : 0f;
        Set("hScrollVisible", viewport.Horizontal);
        Set("vScrollVisible", viewport.Vertical);
        Set("scrollOuterWidth", outerWidth);
        Set("scrollOuterHeight", outerHeight);
        Set("scrollOuterRectClipped", new Rect(fixedWidth, 0f, outerWidth, outerHeight));
        Set("hScrollMax", maximumX);
        Set("vScrollMax", maximumY);
        Vector2 position = (Vector2)gridFields["scrollPosition"].GetValue(__instance)!;
        position.x = Mathf.Clamp(position.x, 0f, maximumX);
        position.y = Mathf.Clamp(position.y, 0f, maximumY);
        Set("scrollPosition", position);
        Set("hScrollRect", viewport.Horizontal ? new Rect(bounds.x + fixedWidth, bounds.yMax - horizontalHeight, outerWidth, horizontalHeight) : Rect.zero);
        Set("vScrollRect", viewport.Vertical ? new Rect(bounds.xMax - verticalWidth, bounds.y + fixedHeight, verticalWidth, outerHeight) : Rect.zero);
        Set("fixedPanelRect", new Rect(bounds.x, bounds.y, fixedWidth, fixedHeight));
        Set("columnsPanelRect", new Rect(bounds.x + fixedWidth, bounds.y, outerWidth, fixedHeight));
        Set("tableBodyOuterRect", new Rect(bounds.x, bounds.y + fixedHeight, fixedWidth + outerWidth, outerHeight));
        Set("namesPanelRect", new Rect(0f, 0f, fixedWidth, outerHeight));
        if (!viewport.Horizontal && ScrollDrags.TryGetValue(__instance, out ScrollDrag? drag) && drag.Control != 0)
        {
            if (GUIUtility.hotControl == drag.Control) GUIUtility.hotControl = 0;
            drag.Control = 0;
        }
    }

    private static void ResolverPostfix(PawnColumnDef column, object __result)
    {
        if (__result == null || column is not WorkGiverColumnDef && column.Worker is not PawnColumnWorker_WorkPriority) return;
        if (column is WorkGiverColumnDef && workerClass.GetValue(__result) as Type != workWorker)
        {
            workerClass.SetValue(__result, workWorker);
            workerCache.SetValue(__result, null);
        }
        object? worker = workerProperty.GetValue(__result);
        if (worker == null || !workWorker.IsInstanceOfType(worker)) return;
        Columns.Remove(worker);
        Columns.Add(worker, new Binding(column));
    }

    private static bool ColumnPrefix(object __instance, ref PawnColumnDef __result)
    {
        if (!Columns.TryGetValue(__instance, out Binding? binding)) return true;
        __result = binding.Column;
        return false;
    }

    private static PawnColumnDef? Definition(object worker)
        => Columns.TryGetValue(worker, out Binding? binding) ? binding.Column : columnProperty.GetValue(worker) as PawnColumnDef;

    private static bool CanEdit(Pawn pawn, PawnColumnDef column)
    {
        if (pawn.Dead || !pawn.RaceProps.Humanlike || pawn.workSettings?.EverWork != true || pawn.WorkTypeIsDisabled(column.workType)) return false;
        if (column is not WorkGiverColumnDef leaf) return true;
        if (!WorkCatalog.Standalone(leaf.Giver) && WorkPriorities.Parent(pawn, column.workType) == 0 || pawn.WorkTagIsDisabled(leaf.Giver.workTags)) return false;
        return leaf.Giver.requiredCapacities.All(c => pawn.health.capacities.CapableOf(c));
    }

    private static int Value(Pawn pawn, PawnColumnDef column) => column is WorkGiverColumnDef leaf
        ? WorkPriorities.Effective(pawn, leaf.Giver) : WorkPriorities.Summary(pawn, column.workType).Priority;

    private static bool ValuePrefix(object __instance, Pawn pawn, ref object __result)
    {
        PawnColumnDef? column = Definition(__instance);
        if (column is not WorkGiverColumnDef) return true;
        __result = Value(pawn, column);
        return false;
    }

    private static bool SetPrefix(object __instance, Pawn pawn, object value, PawnTable table)
    {
        PawnColumnDef? column = Definition(__instance);
        if (!WorkTableAccess.IsWork(table) || column == null) return true;
        if (CanEdit(pawn, column)) Set(pawn, column, Convert.ToInt32(value));
        return false;
    }

    private static bool VisiblePrefix(object __instance, Pawn pawn, ref bool __result)
    {
        PawnColumnDef? column = Definition(__instance);
        if (column is not WorkGiverColumnDef) return true;
        __result = CanEdit(pawn, column);
        return false;
    }

    private static void Set(Pawn pawn, PawnColumnDef column, int? value)
    {
        if (column is WorkGiverColumnDef leaf) WorkPriorityComponent.Current?.SetOverride(pawn, leaf.Giver, value);
        else if (value.HasValue) WorkPriorities.SetParent(pawn, column.workType, PriorityRules.Limit(value.Value, WorkReworkMod.MaximumPriority));
        else WorkPriorityComponent.Current?.ResetCategory(pawn, column.workType);
    }

    private static void ModelPostfix(object __instance)
    {
        if (modelTable.GetValue(__instance) is PawnTable table && WorkTableAccess.IsWork(table)) allowScroll.SetValue(__instance, true);
    }

    private static bool CellPrefix(object __instance, Rect rect, object column, PawnTable table)
    {
        if (!WorkTableAccess.IsWork(table) || !workWorker.IsInstanceOfType(__instance)) return true;
        PawnColumnDef? definition = Definition(__instance);
        object? group = groupProperty.GetValue(column);
        if (definition == null || group == null) return true;
        var members = ((IEnumerable<Pawn>)groupPawns.GetValue(group)!).Where(p => CanEdit(p, definition)).ToList();
        if (members.Count == 0) return false;
        int first = Value(members[0], definition);
        bool uniform = members.All(p => Value(p, definition) == first);
        bool childMixed = definition is not WorkGiverColumnDef && members.Any(p => WorkPriorities.Summary(p, definition.workType).Mixed);
        Rect box = new(rect.center.x - 12.5f, rect.y + 2.5f, 25f, 25f);
        Color oldColor = GUI.color;
        GameFont oldFont = Text.Font;
        TextAnchor oldAnchor = Text.Anchor;
        try
        {
            GUI.color = Color.white;
            Widgets.DrawBoxSolidWithOutline(box, new Color(0.14f, 0.16f, 0.18f, 0.9f), new Color(0.45f, 0.5f, 0.52f, 0.55f));
            if (Mouse.IsOver(box)) Widgets.DrawHighlight(box);
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = definition is WorkGiverColumnDef && !WorkCatalog.Standalone(((WorkGiverColumnDef)definition).Giver) ? GameFont.Small : GameFont.Medium;
            GUI.color = uniform ? WidgetsWork.ColorOfPriority(first) : Color.gray;
            if (!uniform) Widgets.Label(box.ContractedBy(-3f), "…");
            else if (Find.PlaySettings.useWorkPriorities && first > 0) Widgets.Label(box.ContractedBy(-3f), first.ToString());
            else if (!Find.PlaySettings.useWorkPriorities && first > 0) { GUI.color = Color.white; GUI.DrawTexture(box, WidgetsWork.WorkBoxCheckTex); }
            if (childMixed)
            {
                GUI.color = Color.white;
                Text.Font = GameFont.Tiny;
                Text.Anchor = TextAnchor.UpperLeft;
                Widgets.Label(new Rect(box.xMax - 6f, box.y - 2f, 10f, 16f), "*");
            }
            TooltipHandler.TipRegion(rect, "WR_GroupPriorityTip".Translate(members.Count));
            Event current = Event.current;
            if (current.type == EventType.mouseDown && Mouse.IsOver(box))
            {
                Action<int?> apply = value =>
                {
                    foreach (Pawn pawn in members.Where(p => CanEdit(p, definition))) Set(pawn, definition, value);
                    notifyValues.Invoke(column, null);
                    table.SetDirty();
                };
                if (current.button == 2) ShowMenu(definition, apply);
                else if (current.button == 1 && current.control && definition is WorkGiverColumnDef leaf)
                {
                    if (!WorkCatalog.Standalone(leaf.Giver)) apply(null);
                }
                else if (current.button == 0 || current.button == 1)
                    apply(Find.PlaySettings.useWorkPriorities ? uniform ? PriorityRules.Cycle(first, current.button == 0, WorkReworkMod.MaximumPriority) : PriorityRules.Limit(3, WorkReworkMod.MaximumPriority)
                        : uniform && first > 0 ? 0 : PriorityRules.Limit(3, WorkReworkMod.MaximumPriority));
                else return false;
                current.Use();
            }
        }
        finally { GUI.color = oldColor; Text.Font = oldFont; Text.Anchor = oldAnchor; }
        return false;
    }

    private static void ShowMenu(PawnColumnDef column, Action<int?> apply)
    {
        var options = new List<FloatMenuOption>();
        if (Find.PlaySettings.useWorkPriorities)
            for (int i = 0; i <= WorkReworkMod.MaximumPriority; i++)
            {
                int value = i;
                options.Add(new FloatMenuOption(value == 0 ? "WR_Disabled".Translate() : "WR_PriorityValue".Translate(value), () => apply(value)));
            }
        else
        {
            options.Add(new FloatMenuOption("WR_Enabled".Translate(), () => apply(PriorityRules.Limit(3, WorkReworkMod.MaximumPriority))));
            options.Add(new FloatMenuOption("WR_Disabled".Translate(), () => apply(0)));
        }
        if (column is not WorkGiverColumnDef || !WorkCatalog.Standalone(((WorkGiverColumnDef)column).Giver))
            options.Add(new FloatMenuOption("WR_GroupInherit".Translate(), () => apply(null)));
        Find.WindowStack.Add(new FloatMenu(options));
    }
}

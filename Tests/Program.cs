using WorkRework;

int passed = 0;
void Equal(string scenario, int expected, int actual)
{
    if (expected != actual) throw new Exception($"{scenario}: expected {expected}, got {actual}");
    passed++;
}
void True(string scenario, bool actual)
{
    if (!actual) throw new Exception(scenario);
    passed++;
}

Equal("New children inherit unchanged native priority", 4, PriorityRules.Effective(4, null, true));
Equal("Child priority 1 overrides parent 4", 1, PriorityRules.Effective(4, 1, true));
Equal("Child priority 9 remains available", 9, PriorityRules.Effective(2, 9, true));
Equal("Explicit child zero is disabled", 0, PriorityRules.Effective(2, 0, true));
Equal("Disabled parent blocks explicit priority 1", 0, PriorityRules.Effective(0, 1, true));
Equal("Disabled parent blocks inherited work", 0, PriorityRules.Effective(0, null, true));
Equal("Simple mode treats positive priority uniformly", 3, PriorityRules.Effective(4, 9, false));
Equal("Simple mode keeps disabled children off", 0, PriorityRules.Effective(4, 0, false));
Equal("Simple mode still honors disabled parent", 0, PriorityRules.Effective(0, 1, false));
int rememberedChild = 9;
Equal("Toggling simple mode does not replace stored priority", 3, PriorityRules.Effective(4, rememberedChild, false));
Equal("Returning to manual mode restores child 9", 9, PriorityRules.Effective(4, rememberedChild, true));
Equal("Disabling parent keeps child setting stored", 0, PriorityRules.Effective(0, rememberedChild, true));
Equal("Reactivating parent recovers child setting", 9, PriorityRules.Effective(4, rememberedChild, true));
Equal("Resetting exception resumes current parent", 6, PriorityRules.Effective(6, null, true));
Equal("Left click from off reaches 9", 9, PriorityRules.Cycle(0, true));
Equal("Left click from 1 reaches off", 0, PriorityRules.Cycle(1, true));
Equal("Right click from off reaches 1", 1, PriorityRules.Cycle(0, false));
Equal("Right click from 9 reaches off", 0, PriorityRules.Cycle(9, false));
True("Priority 1 precedes priority 9 regardless of native category order", PriorityRules.Compare(1, 0, 30, 50, 9, 1000, 0, 0) < 0);
True("At equal priority native category order is preserved", PriorityRules.Compare(4, 100, 1, 50, 4, 50, 0, 0) < 0);
True("Tied categories preserve their stable original index", PriorityRules.Compare(4, 50, 1, 0, 4, 50, 2, 0) < 0);
True("Tasks at equal priority preserve native sub-order", PriorityRules.Compare(4, 50, 1, 2, 4, 50, 1, 3) < 0);
True("Urgent work at equal priority retains emergency pass", PriorityRules.InEmergencyPass(true, 4, 4));
True("Lower-priority urgent work waits behind normal work", !PriorityRules.InEmergencyPass(true, 9, 1));
True("Non-urgent work never enters emergency pass", !PriorityRules.InEmergencyPass(false, 1, 9));
True("Only urgent work still runs when no normal work is enabled", PriorityRules.InEmergencyPass(true, 9, int.MaxValue));
True("Disabled emergency work stays excluded", !PriorityRules.InEmergencyPass(true, 0, 4));
PrioritySummary mixed = PriorityRules.Summarize(new[] { 4, 1, 0 }, 4);
Equal("Parent summary ignores zero when selecting its minimum", 1, mixed.Priority);
True("Parent marks unequal children including zero", mixed.Mixed);
PrioritySummary equal = PriorityRules.Summarize(new[] { 3, 3, 3 }, 3);
Equal("Uniform child summary", 3, equal.Priority);
True("Uniform children have no asterisk", !equal.Mixed);
PrioritySummary allOff = PriorityRules.Summarize(new[] { 0, 0, 0 }, 4);
Equal("All children disabled produce zero", 0, allOff.Priority);
True("All children equally disabled are not mixed", !allOff.Mixed);
Equal("Empty category retains native fallback", 4, PriorityRules.Summarize(Array.Empty<int>(), 4).Priority);
var catalog = new[] { new WorkCategory("Build", new[] { "Construct", "Repair", "Deconstruct" }), new WorkCategory("Cook", new[] { "Meals", "Butcher" }) };
var original = new WorkLayout(catalog);
var draft = original.Clone();
True("Draft extracts repair", draft.Extract("Repair"));
True("Cancel leaves the source layout unchanged", original.Children["Build"].Contains("Repair") && !original.IsStandalone("Repair"));
True("Extraction creates exactly one copy", draft.OrderedTasks().Count(t => t == "Repair") == 1);
True("Extracted child leaves group", !draft.Children["Build"].Contains("Repair"));
True("Cannot extract twice", !draft.Extract("Repair"));
True("Cannot reparent into foreign origin", !draft.Reintegrate("Repair", "Cook"));
True("Invalid return keeps standalone column", draft.IsStandalone("Repair"));
WorkLayout.Move(draft.Roots, draft.Roots.IndexOf(WorkLayout.Task("Repair")), 0);
True("Standalone task can precede its origin", draft.OrderedTasks().First() == "Repair");
True("Return to origin succeeds", draft.Reintegrate("Repair", "Build"));
True("Return removes standalone column", !draft.IsStandalone("Repair"));
True("Return appends task to original children", draft.Children["Build"].Last() == "Repair");
WorkLayout.Move(draft.Children["Build"], 2, 0);
True("Child reorder changes effective tie order", draft.OrderedTasks().First() == "Repair");
WorkLayout.Move(draft.Roots, 0, draft.Roots.Count);
True("Category moves with all children", draft.OrderedTasks().SequenceEqual(new[] { "Meals", "Butcher", "Repair", "Construct", "Deconstruct" }));
True("Priority outranks any chosen order", WorkLayout.Compare(2, 100, 6, 0) < 0);
True("Chosen order breaks priority ties", WorkLayout.Compare(4, 0, 4, 100) < 0);
var restored = new WorkLayout(catalog, draft.Roots, draft.Children.Values.SelectMany(c => c));
True("Serialized order restores exactly", restored.OrderedTasks().SequenceEqual(draft.OrderedTasks()));
var damaged = new WorkLayout(catalog, new[] { "T:Repair", "T:Repair", "G:Missing", "bad", "G:Build", "G:Build" }, new[] { "Repair", "Construct", "Construct", "Missing" });
True("Normalization eliminates duplicates and absent defs", damaged.Roots.Count == 3 && damaged.OrderedTasks().Distinct().Count() == 5);
True("Normalization appends new tasks to their origin", damaged.Children["Cook"].SequenceEqual(new[] { "Meals", "Butcher" }));
True("Empty save defaults preserve catalog order", new WorkLayout(catalog).OrderedTasks().SequenceEqual(catalog.SelectMany(c => c.Tasks)));
int preserved = PriorityRules.Effective(4, 9, true);
Equal("Extracted task retains stored numeric value in simple mode", 9, preserved);
Equal("Standalone ignores former parent gate", 9, PriorityRules.Effective(1, preserved, true));
Equal("Off task remains off after extraction", 0, PriorityRules.Effective(1, PriorityRules.Effective(0, 1, true), true));
var extracted = original.Clone();
extracted.Extract("Repair");
var bases = new Dictionary<string, int> { ["Build"] = 4, ["Cook"] = 2 };
var exceptions = new Dictionary<string, int> { ["Repair"] = 9, ["Butcher"] = 1, ["AbsentDef"] = 7 };
var movement = OrganizationPriorityMigration.Build(original, extracted, bases, exceptions);
Equal("Migration preserves child override 9", 9, movement.Overrides["Repair"]);
True("Extraction does not alter other inheritance", !movement.Overrides.ContainsKey("Construct"));
Equal("Migration preserves unrelated exceptions", 1, movement.Overrides["Butcher"]);
Equal("Migration retains absent Def data", 7, movement.Overrides["AbsentDef"]);
True("Extraction leaves group bases unchanged", movement.ParentChanges.Count == 0);
var inheritedMove = OrganizationPriorityMigration.Build(original, extracted, bases, new Dictionary<string, int>());
Equal("Extraction freezes inherited priority", 4, inheritedMove.Overrides["Repair"]);
bases["Build"] = 0;
var blockedMove = OrganizationPriorityMigration.Build(original, extracted, bases, exceptions);
Equal("Extraction of blocked override freezes effective zero", 0, blockedMove.Overrides["Repair"]);
var returning = OrganizationPriorityMigration.Build(extracted, original, bases, movement.Overrides);
Equal("Return reopens zero group at returning task value", 9, returning.ParentChanges["Build"]);
Equal("Return preserves all other disabled children", 0, returning.Overrides["Construct"]);
Equal("Return preserves all disabled siblings", 0, returning.Overrides["Deconstruct"]);
Equal("Return preserves active independent priority", 9, returning.Overrides["Repair"]);
Equal("Reopened group's other child stays effectively zero", 0, PriorityRules.Effective(returning.ParentChanges["Build"], returning.Overrides["Construct"], true));
bases["Build"] = 3;
var regularReturn = OrganizationPriorityMigration.Build(extracted, original, bases, movement.Overrides);
True("Return into enabled group preserves its base", regularReturn.ParentChanges.Count == 0);
Equal("Return creates explicit exception", 9, regularReturn.Overrides["Repair"]);
True("Return into enabled group preserves sibling inheritance", !regularReturn.Overrides.ContainsKey("Construct"));
var reorderOnly = OrganizationPriorityMigration.Build(original, draft, bases, exceptions);
True("Reordering alone never changes priority data", reorderOnly.ParentChanges.Count == 0 && reorderOnly.Overrides.OrderBy(p => p.Key).SequenceEqual(exceptions.OrderBy(p => p.Key)));
// Regression: successive applied changes must preserve each task's effective
// priority for multiple pawn profiles, in both display modes, across model reloads.
var regressionLayout = new WorkLayout(catalog);
var profiles = new[]
{
    (Parents: new Dictionary<string, int> { ["Build"] = 4, ["Cook"] = 2 }, Values: new Dictionary<string, int>()),
    (Parents: new Dictionary<string, int> { ["Build"] = 2, ["Cook"] = 9 }, Values: new Dictionary<string, int> { ["Repair"] = 1, ["Butcher"] = 0 }),
    (Parents: new Dictionary<string, int> { ["Build"] = 0, ["Cook"] = 3 }, Values: new Dictionary<string, int> { ["Repair"] = 9, ["Meals"] = 6 })
};
Action<WorkLayout>[] changes =
{
    l => l.Extract("Repair"),
    l => { l.Extract("Butcher"); WorkLayout.Move(l.Roots, l.Roots.IndexOf(WorkLayout.Task("Butcher")), 0); },
    l => l.Reintegrate("Repair", "Build"),
    l => l.Extract("Construct"),
    l => l.Reintegrate("Butcher", "Cook"),
    l => l.Reintegrate("Construct", "Build")
};
int step = 0;
foreach (Action<WorkLayout> change in changes)
{
    step++;
    var next = regressionLayout.Clone();
    change(next);
    for (int p = 0; p < profiles.Length; p++)
    {
        var profile = profiles[p];
        var migration = OrganizationPriorityMigration.Build(regressionLayout, next, profile.Parents, profile.Values);
        var nextParents = new Dictionary<string, int>(profile.Parents);
        foreach (var pair in migration.ParentChanges) nextParents[pair.Key] = pair.Value;
        foreach (bool manual in new[] { true, false })
            foreach (string task in regressionLayout.OrderedTasks())
            {
                int before = PriorityRules.Effective(regressionLayout.IsStandalone(task) ? 1 : profile.Parents[regressionLayout.Origin(task)],
                    profile.Values.TryGetValue(task, out int v) ? v : null, manual);
                int after = PriorityRules.Effective(next.IsStandalone(task) ? 1 : nextParents[next.Origin(task)],
                    migration.Overrides.TryGetValue(task, out int w) ? w : null, manual);
                Equal($"Sequence step {step}, pawn {p}, task {task}, manual {manual}", before, after);
            }
        profiles[p] = (nextParents, migration.Overrides);
    }
    regressionLayout = new WorkLayout(catalog, next.Roots, next.Children.Values.SelectMany(c => c));
    True($"Sequence step {step} retains order and every unique task across model reload",
        regressionLayout.OrderedTasks().SequenceEqual(next.OrderedTasks()) && regressionLayout.OrderedTasks().Distinct().Count() == 5);
}
var resetSource = regressionLayout.Clone();
resetSource.Extract("Repair");
resetSource.Extract("Butcher");
WorkLayout.Move(resetSource.Roots, 0, resetSource.Roots.Count);
var resetDraft = new WorkLayout(catalog);
True("Reset draft restores vanilla order and removes standalone columns", resetDraft.Roots.SequenceEqual(original.Roots) && resetDraft.OrderedTasks().SequenceEqual(original.OrderedTasks()));
True("Cancel reset preserves source organization", resetSource.IsStandalone("Repair") && resetSource.IsStandalone("Butcher"));
foreach (var profile in profiles)
{
    var migration = OrganizationPriorityMigration.Build(resetSource, resetDraft, profile.Parents, profile.Values);
    var nextParents = new Dictionary<string, int>(profile.Parents);
    foreach (var pair in migration.ParentChanges) nextParents[pair.Key] = pair.Value;
    foreach (bool manual in new[] { true, false })
        foreach (string task in resetSource.OrderedTasks())
        {
            int before = PriorityRules.Effective(resetSource.IsStandalone(task) ? 1 : profile.Parents[resetSource.Origin(task)], profile.Values.TryGetValue(task, out int v) ? v : null, manual);
            int after = PriorityRules.Effective(nextParents[resetDraft.Origin(task)], migration.Overrides.TryGetValue(task, out int w) ? w : null, manual);
            Equal($"Reset organization preserves {task} in manual mode {manual}", before, after);
        }
}
Equal("Lower cap reduces parent or child 9 to 4", 4, PriorityRules.Limit(9, 4));
Equal("Lower cap leaves zero disabled", 0, PriorityRules.Limit(0, 1));
Equal("Lower cap keeps an existing higher-priority value", 2, PriorityRules.Limit(2, 4));
Equal("Raise after conversion cannot recover the old number", 4, PriorityRules.Limit(PriorityRules.Limit(9, 4), 9));
Equal("Configured left cycle starts at chosen maximum", 4, PriorityRules.Cycle(0, true, 4));
Equal("Configured right cycle stops at chosen maximum", 0, PriorityRules.Cycle(4, false, 4));
Equal("Maximum 1 cycles to disabled", 0, PriorityRules.Cycle(1, true, 1));
Equal("New pawn receives configured default", 2, PriorityRules.InitialAssignment(3, 4, true, 2));
Equal("New pawn keeps inactive work off", 0, PriorityRules.InitialAssignment(0, 4, true, 2));
Equal("Existing pawn reactivation retains vanilla 3", 3, PriorityRules.InitialAssignment(3, 4, false, 2));
Equal("Existing pawn retains priority 1", 1, PriorityRules.InitialAssignment(1, 4, false, 2));
Equal("Default cannot exceed configured maximum", 4, PriorityRules.InitialAssignment(3, 4, true, 9));
Equal("Reactivation at maximum 1 stays within cap", 1, PriorityRules.InitialAssignment(3, 1, false, 3));
void CheckViewport(string label, float contentWidth, float contentHeight, bool horizontal, bool vertical)
{
    ScrollViewport viewport = ScrollViewport.Calculate(contentWidth, contentHeight, 1000f, 400f, 16f, 16f);
    True(label, viewport.Horizontal == horizontal && viewport.Vertical == vertical);
    True(label + " reserves only visible bars", viewport.Width == (vertical ? 984f : 1000f) && viewport.Height == (horizontal ? 384f : 400f));
}
CheckViewport("Exact fit has no scrollbars", 1000f, 400f, false, false);
CheckViewport("Near fit does not reserve a phantom vertical gutter", 990f, 390f, false, false);
CheckViewport("Width-only overflow has horizontal scrollbar only", 1100f, 200f, true, false);
CheckViewport("Height-only overflow has vertical scrollbar only", 800f, 600f, false, true);
CheckViewport("Vertical bar may cause genuine horizontal overflow", 990f, 600f, true, true);
CheckViewport("Horizontal bar may cause genuine vertical overflow", 1100f, 390f, true, true);
CheckViewport("Subpixel rounding does not create bars", 1000.5f, 400.5f, false, false);
CheckViewport("Collapsed rows and columns remove both bars", 500f, 200f, false, false);
ScrollViewport narrow = ScrollViewport.Calculate(1100f, 200f, 1000f, 400f, 16f, 16f);
ScrollViewport widened = ScrollViewport.Calculate(1100f, 200f, 1200f, 400f, 16f, 16f);
True("Resize removes unnecessary horizontal scroll without retaining old state", narrow.Horizontal && !widened.Horizontal && !widened.Vertical);
ScrollViewport tiny = ScrollViewport.Calculate(100f, 100f, 10f, 10f, 16f, 16f);
True("Tiny viewport never produces negative drawing sizes", tiny.Width == 0f && tiny.Height == 0f);
Console.WriteLine($"Priority, layout, migration and viewport rules: {passed} scenario assertions passed. No game/runtime or save-load simulation claimed.");
if (args.Length >= 1)
    GameIlContracts.Check(args[0]);
if (args.Length >= 2)
    GroupedContracts.Check(args[1]);

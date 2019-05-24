
// Copyright Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net

/* TODO:
 * Write a parallel access test to check consistency.
 */

using CB.Parallelism;

public partial class TestHelpers
{
	public void Test_CB_Parallelism_PositionList_cs()
	{
		{
			var list = new PositionList<int>(10);
			var p0 = list.GetStartPosition();
			Release.Assert(!p0.DebugIndex.HasValue);
			var i0 = list[p0];
			Release.Assert(i0 == 0);
			list.Add(1);
			Release.Assert(p0.DebugIndex == 0);
			var i1 = list[p0];
			Release.Assert(i1 == 1);
			list.Insert(p0, RelativePosition.After, 2);
			Release.Assert(p0.DebugIndex == 0);
			var i2 = list[p0];
			Release.Assert(i2 == 1);
			list.Insert(p0, RelativePosition.Before, 3);
			Release.Assert(p0.DebugIndex == 1);
			var i3 = list[p0];
			Release.Assert(i3 == 1);
			var tab = list.ToArray();
			Release.Assert(tab.Length == 3 && tab[0] == 3 && tab[1] == 1 && tab[2] == 2);
		}
		{
			var list = new PositionList<int>(10);
			var p0 = list.GetStartPosition();
			Release.Assert(!p0.PointsAtAnItem);
			var p1 = list.InsertAndGetNewPosition(p0, RelativePosition.After, 1);
			Release.Assert(p1.DebugIndex == 0);
			list.Clear();
			Release.Assert(list.IsEmpty);
			Release.Assert(p0.DebugIndex == null);
			Release.Assert(!p1.PointsAtAnItem);
			var p2 = list.InsertAndGetNewPosition(p1, RelativePosition.Before, 2);
			Release.Assert(p2 == p1);
			var p3 = list.InsertAndGetNewPosition(p2, RelativePosition.Before, 3);
			Release.Assert(p3 < p2);
		}
		{
			var list = new PositionList<string>() { "a", "c" };
			var posA = list.GetStartPosition();
			Release.Assert(posA.IsAtListStart);
			var posB = list.InsertAndGetNewPosition(posA, RelativePosition.After, "b");
			Release.Assert(posB.DebugIndex == 1);
			var tab = list.ToArray();
			Release.Assert(tab.Length == 3 && tab[0] == "a" && tab[1] == "b" && tab[2] == "c");
			var posB2 = list.MoveItem(posB, list.GetEndPosition(), RelativePosition.After, true);
			Release.Assert(posB2.DebugIndex == 2);
			Release.Assert(posB == posB2);
			tab = list.ToArray();
			Release.Assert(tab.Length == 3 && tab[0] == "a" && tab[1] == "c" && tab[2] == "b");
		}
		{
			PositionString positionString = "ac";
			var posB = positionString.AddItemAndGetPosition('b');
			Release.Assert(posB.DebugIndex == 2);
			var posB2 = positionString.MoveItem(posB, posB, RelativePosition.Before, true);
		}
		{
			var list = new PositionList<string>() { "b", "c" };
			Position position = list.GetFirstOccurenceOf("c");
			list.InsertBefore(list.GetStartPosition(), "a");
			list[position] = "c2";
			var tab = list.ToArray();
			Release.Assert(tab.Length == 3 && tab[0] == "a" && tab[1] == "b" && tab[2] == "c2");
		}
	}
}
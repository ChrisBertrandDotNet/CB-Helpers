
// Copyright Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net

using CB.Parallelism;

public partial class TestHelpers
{

	public void test_CB_Parallelism_ConsistentData_cs()
	{
		test1_CB_Parallelism_ConsistentData_cs();
		Test2_CB_Parallelism_ConsistentData_cs();
	}

	#region test 1

	void test1_CB_Parallelism_ConsistentData_cs()
	{
		var color1 = new MyColor(100, 100, 100);
		var color2 = color1.Lighten(50); // color2 is consistent, even in presence of parallel tasks.
	}

	public class MyColorData
	{
		public readonly int Red, Green, Blue;

		public MyColorData(int red, int green, int blue)
		{ Red = red; Green = green; Blue = blue; }
	}

	public class MyColor : Consistent<MyColorData>
	{
		public MyColor(int red, int green, int blue)
			: base(new MyColorData(red, green, blue))
		{ }

		public MyColorData Lighten(int difference)
		{
			var current = this.Data;
			var Lightened = new MyColorData(current.Red + difference, current.Green + difference, current.Blue + difference);
			this.Data = Lightened;
			return Lightened;
		}
	}

	#endregion test 1

	#region test2

	void Test2_CB_Parallelism_ConsistentData_cs()
	{		
		 var range1 = new MyRange(100, 200);
		var range2 = range1.EnlargeRange(10); // Gets the new (consistent) range: [ 90 ; 210 ].
	}

	public class MyRangeData
	{
		public readonly int Minimum, Maximum;
		public MyRangeData(int minimum, int maximum)
		{
			if (maximum < minimum)
				throw new InconsistentArgumentsException();
			Minimum = minimum; Maximum = maximum;
		}
	}

	public class MyRange : Consistent<MyRangeData>
	{
		public MyRange(int minimum, int maximum)
			: base(new MyRangeData(minimum, maximum))
		{ }

		public MyRangeData EnlargeRange(int difference)
		{
			var current = this.Data;
			var enlarged = new MyRangeData(current.Minimum - difference, current.Maximum + difference);
			this.Data = enlarged;
			return enlarged;
		}
	}

	#endregion test2
}
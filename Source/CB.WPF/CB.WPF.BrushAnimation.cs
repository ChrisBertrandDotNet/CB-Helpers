using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

// from https://stackoverflow.com/a/29659723 with modifications.

/* Example in XAML:
<EventTrigger RoutedEvent="Loaded">
    <BeginStoryboard>
        <Storyboard >
            <local:BrushAnimation Storyboard.TargetName="border"
                                  Storyboard.TargetProperty="Background" 
                                  Duration="0:0:5" From="Red" 
                                  RepeatBehavior="Forever" AutoReverse="True" >
                <local:BrushAnimation.To>
                    <LinearGradientBrush EndPoint="0.5,1" StartPoint="0.5,0">
                        <GradientStop Color="#FF00FF2E" Offset="0.005"/>
                        <GradientStop Color="#FFC5FF00" Offset="1"/>
                        <GradientStop Color="Blue" Offset="0.43"/>
                    </LinearGradientBrush>
                </local:BrushAnimation.To>
            </local:BrushAnimation>
        </Storyboard>
    </BeginStoryboard>
</EventTrigger>
*/

/* Example in C#:
var animation = new BrushAnimation
{
    From = Brushes.Red,
    To = new LinearGradientBrush (Colors.Green, Colors.Yellow, 45),
    Duration = new Duration(TimeSpan.FromSeconds(5)),
};
animation.Completed += new EventHandler(animation_Completed);
Storyboard.SetTarget(animation, border);
Storyboard.SetTargetProperty(animation, new PropertyPath("Background"));

var sb = new Storyboard();
sb.Children.Add(animation);
sb.Begin();
 */


namespace CB.WPF
{

	public class BrushAnimation : AnimationTimeline
	{
		public override Type TargetPropertyType
		{
			get
			{
				return typeof(Brush);
			}
		}

		public override object GetCurrentValue(object defaultOriginValue,
											   object defaultDestinationValue,
											   AnimationClock animationClock)
		{
			return GetCurrentValue(defaultOriginValue as Brush,
								   defaultDestinationValue as Brush,
								   animationClock);
		}
		public object GetCurrentValue(Brush defaultOriginValue,
									  Brush defaultDestinationValue,
									  AnimationClock animationClock)
		{
			if (!animationClock.CurrentProgress.HasValue)
				return Brushes.Transparent;

			//use the standard values if From and To are not set 
			//(it is the value of the given property)
			defaultOriginValue = this.From ?? defaultOriginValue;
			defaultDestinationValue = this.To ?? defaultDestinationValue;

			if (animationClock.CurrentProgress.Value == 0)
				return defaultOriginValue;
			if (animationClock.CurrentProgress.Value == 1)
				return defaultDestinationValue;

			return new VisualBrush(new Border()
			{
				Width = 1,
				Height = 1,
				Background = defaultOriginValue,
				Child = new Border()
				{
					Background = defaultDestinationValue,
					Opacity = animationClock.CurrentProgress.Value,
				}
			});
		}

		public BrushAnimation()
		{ }

		public BrushAnimation(Brush from, Brush to, Duration duration)			
		{
			this.From = from;
			this.To = to;
			base.Duration = duration;
		}

		protected override Freezable CreateInstanceCore()
		{
			return new BrushAnimation();
		}

		//we must define From and To, AnimationTimeline does not have this properties
		public Brush From
		{
			get { return (Brush)GetValue(FromProperty); }
			set { SetValue(FromProperty, value); }
		}
		public Brush To
		{
			get { return (Brush)GetValue(ToProperty); }
			set { SetValue(ToProperty, value); }
		}

		public static readonly DependencyProperty FromProperty =
			DependencyProperty.Register("From", typeof(Brush), typeof(BrushAnimation));
		public static readonly DependencyProperty ToProperty =
			DependencyProperty.Register("To", typeof(Brush), typeof(BrushAnimation));
	}
}

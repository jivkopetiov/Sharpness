using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharpness.Tests
{
    [TestFixture]
    public class MethodInvocationTests
    {
        [TestCase("[hud show:animated]", Result = "hud.show(animated)")]
        [TestCase("[UIImageView imageViewWithFrame:self.bounds]", Result = "UIImageView.imageViewWithFrame(this.Bounds)")]
        [TestCase("[self setCurrentCoverAtIndex:index animated:NO]", Result = "this.setCurrentCoverAtIndex(index, false)")]
        [TestCase("[self.coverflowDataSource numberOfCoversInCoverflowView:self]", Result = "this.coverflowDataSource.numberOfCoversInCoverflowView(this)")]
        [TestCase("[v.Layer removeAllAnimations]", Result = "v.Layer.removeAllAnimations()")]
        [TestCase("[self _calculatedIndexWithContentOffset:*targetContentOffset]", Result = "this._calculatedIndexWithContentOffset(targetContentOffset)")]
        public string Tests(string input)
        {
            return new SharpnessParser().Parse(input);
        }

        [TestCase("[UIColor colorWithWhite:0 alpha:0.8]", Result = "UIColor.FromWhiteAlpha(0, 0.8)")]
        [TestCase("[UIColor colorWithWhite:102/255. alpha:1]", Result = "UIColor.FromWhiteAlpha(102/255., 1)")]
        public string UIColor(string input)
        {
            return new SharpnessParser().Parse(input);
        }
        // [[UIColor alloc] initWithWhite:1.f alpha:1.f] -> UIColor.FromWhiteAlpha(1.f, 1.f)

        [TestCase("[UIFont boldSystemFontOfSize:14]", Result = "UIFont.BoldSystemFontOfSize(14)")]
        [TestCase("[UIFont systemFontOfSize:fontSize]", Result = "UIFont.SystemFontOfSize(fontSize)")]
        public string UIFont(string input)
        {
            return new SharpnessParser().Parse(input);
        }

        [TestCase("UIImageView *iv = [[UIImageView alloc] initWithFrame:CGRectMake(0,0, NOB_SIZE, NOB_SIZE)];",
            Result = "UIImageView iv = new UIImageView(new RectangleF(0,0, NOB_SIZE, NOB_SIZE));")]
        public string UIImageView(string input)
        {
            return new SharpnessParser().Parse(input);
        }
    }
}

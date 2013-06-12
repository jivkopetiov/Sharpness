using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Sharpness
{
    // TODO
    //    [[UIImageView alloc] initWithFrame:
//[[UILabel alloc] initWithFrame:
//imageView.image
//UIViewContentModeCenter
//NSTextAlignmentCenter
//closing ]
//.opacity
//[[UIScreen mainScreen] scale]
//CGImageRef imageRef = CGImageCreateWithImageInRect([image CGImage], scaledRect);
//UIImageOrientationUp
//CGImageRelease(imageRef);
//font pointSize

    public class Enumeration
    {
        public Enumeration()
        {
            Options = new List<string>();
        }

        public string ClassName;
        public string Name;
        public List<string> Options;
    }

    public class Metadata
    {
        public Metadata()
        {
            Enums = new List<Enumeration>();
            Defines = new List<string>();
        }

        public List<Enumeration> Enums;
        public List<string> Defines;

        public string ClassName { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            TransformFile(@"..\..\QBPopupMenu.m", @"C:\Users\jivko\Downloads", null);
            TransformFile(@"..\..\QBPopupMenuItem.m", @"C:\Users\jivko\Downloads", null);
            TransformFile(@"..\..\QBPopupMenuOverlayView.m", @"C:\Users\jivko\Downloads", null);
        }

        private static Metadata TransformFile(string filePath, string outputDir, Metadata metadata)
        {
            var f = new FileInfo(filePath);

            string filename = f.Name;

            if (!filename.EndsWith(".m"))
                throw new InvalidOperationException("Only supports .m files");

            if (metadata == null)
                metadata = new Metadata();

            ParseHeaderFile(f, filename, metadata);

            string result = "";

            foreach (string define in metadata.Defines)
                result += define + Environment.NewLine;

            foreach (var e in metadata.Enums.Where(enu => enu.ClassName == metadata.ClassName))
            {
                result += string.Format(
@"public enum {0} 
{{
    {1}
}}

",
   e.Name, e.Options.JoinStrings("\r\n\t"));
            }

            string text = File.ReadAllText(filePath);
            text = TransformM(text, metadata);
            result += text;
            File.WriteAllText(@"C:\Users\jivko\Downloads\" + metadata.ClassName + ".cs", result);

            return metadata;
        }

        private static void ParseHeaderFile(FileInfo m, string filename, Metadata metadata)
        {
            string className = filename.Substring(0, filename.Length - 2);
            metadata.ClassName = className;
            Console.WriteLine("Parsing header file, classname: " + className);
            string headerPath = Path.Combine(m.DirectoryName, className + ".h");

            if (!File.Exists(headerPath))
            {
                Console.WriteLine("Header file is missing - " + headerPath);
                return;
            }

            string text = File.ReadAllText(headerPath);

            var enumMatches = Regex.Matches(text, @"typedef enum \{([^\}]*?)\}(.*?);");

            foreach (Match match in enumMatches)
            {
                var e = new Enumeration();

                string enumName = match.Groups[2].Value.Trim();
                if (enumName.StartsWith(className, StringComparison.OrdinalIgnoreCase))
                    enumName = enumName.Substring(className.Length);

                string enumBody = match.Groups[1].Value;
                foreach (string line in enumBody.Split('\n'))
                {
                    string l = line.Trim();
                    if (string.IsNullOrEmpty(l))
                        continue;

                    if (l.StartsWith(className + enumName))
                        l = l.Substring(className.Length + enumName.Length);

                    e.Options.Add(l);
                }

                e.ClassName = className;
                e.Name = enumName;
                metadata.Enums.Add(e);
            }

            var defineMatches = Regex.Matches(text, "^#define.*?$", RegexOptions.Multiline);
            foreach (Match match in defineMatches)
                metadata.Defines.Add(ReplaceDefineStatements(match.Value));
        }

        private static string TransformM(string text, Metadata metadata)
        {
            foreach (var e in metadata.Enums)
            {
                foreach (string option in e.Options)
                {
                    string optionCleaned = option;
                    int i = option.IndexOf(" ");
                    if (i != -1)
                        optionCleaned = option.Substring(0, i);

                    optionCleaned = optionCleaned.Trim(',');

                    string token = e.ClassName + e.Name + optionCleaned;

                    text = text.Replace(token, e.Name + "." + optionCleaned);
                }
            }

            text = ReplaceDefineStatements(text);

            text = Regex.Replace(text, @"^@property (\(nonatomic, strong\)|\(nonatomic, assign\))?(.*?) \*?([^\*]*?);$", "public $2 $3;", RegexOptions.Multiline);

            text = Regex.Replace(text, @"/\*[\w\W]*?\*/", "");

            text = Regex.Replace(text, "^@implementation(.*?)$", "public class$1 {", RegexOptions.Multiline);
            text = text.Replace("@end", "}");
            text = Regex.Replace(text, @"\(CGPoint\)\d*?{([\w\W]*?)}", "new PointF($1)");
            text = Regex.Replace(text, @"\(CGRect\)\d*?{([\w\W]*?)}", "new RectangleF($1)");

            text = Regex.Replace(text, @"\[UIFont boldSystemFontOfSize:(\d)+\]", @"UIFont.BoldSystemFontOfSize($1)");
            text = Regex.Replace(text, @"\[UIFont systemFontOfSize:(\d)+\]", @"UIFont.SystemFontOfSize($1)");

            text = Regex.Replace(text, @"CGRectGetMinX\((.*?)\)", "$1.GetMinX()");
            text = Regex.Replace(text, @"CGRectGetMaxX\((.*?)\)", "$1.GetMaxX()");
            text = Regex.Replace(text, @"CGRectGetMinY\((.*?)\)", "$1.GetMinY()");
            text = Regex.Replace(text, @"CGRectGetMaxY\((.*?)\)", "$1.GetMaxY()");
            text = Regex.Replace(text, @"CGRectGetHeight\((.*?)\)", "$1.Height");

            text = Regex.Replace(text, @"isKindOfClass:\[(.*?) class\]", "is $1");

            text = Regex.Replace(text, @"^(\s*?)CGPathMoveToPoint\(([^,]*?),(.*?);$", "$1$2.MoveToPoint($3;", RegexOptions.Multiline);
            text = Regex.Replace(text, @"^(\s*?)CGPathAddArcToPoint\(([^,]*?),(.*?);$", "$1$2.AddArcToPoint($3;", RegexOptions.Multiline);
            text = Regex.Replace(text, @"^(\s*?)CGPathAddLineToPoint\(([^,]*?),(.*?);$", "$1$2.AddLineToPoint($3;", RegexOptions.Multiline);
            text = Regex.Replace(text, @"CGPathCloseSubpath\(([^\)]*?)\);", "$1.CloseSubpath();");
            text = Regex.Replace(text, @"CGContextAddPath\(([^,]*?),", "$1.AddPath(");
            text = Regex.Replace(text, @"CGContextAddRect\(([^,]*?),", "$1.AddRect(");
            text = Regex.Replace(text, @"CGContextSetRGBFillColor\(([^,]*?),", "$1.SetRGBFillColor(");
            text = Regex.Replace(text, @"CGContextDrawLinearGradient\s?\(([^,]*?),", "$1.DrawLinearGradient(");
            
            text = Regex.Replace(text, @"CGContextFillPath\(([^\)]*?)\);", "$1.FillPath();");
            text = Regex.Replace(text, @"CGContextSaveGState\(([^\)]*?)\);", "$1.SaveState();");
            text = Regex.Replace(text, @"CGContextRestoreGState\(([^\)]*?)\);", "$1.RestoreState();");
            text = Regex.Replace(text, @"CGContextClip\(([^\)]*?)\);", "$1.Clip();");
            text = Regex.Replace(text, @"(CGPathRelease|CGGradientRelease|CGColorSpaceRelease)\(([^\)]*?)\);", "$2.Dispose();");

            var evaluator = new MatchEvaluator(match =>
            {
                string v = match.Groups[1].Value;
                return "UIColor." + v[0].ToString().ToUpperInvariant() + v.Substring(1);
            });
            text = Regex.Replace(text, @"\[UIColor ([a-zA-Z]*?)Color\]", evaluator);

            text = Regex.Replace(text, "^//.*?$", "", RegexOptions.Multiline);
            text = Regex.Replace(text, "^(@interface|#import).*?$", "", RegexOptions.Multiline);
            text = text.Trim();
            text = Regex.Replace(text, "\n{3,}", "\n\n");
            text = text.Replace("self.", "this.")
                .Replace("PointFZero", "PointF.Empty")
                .Replace("CGPointMake", "new PointF")
                .Replace("CGRectMake", "new RectangleF")
                .Replace("UIEdgeInsetsMake", "new UIEdgeInsets")
                .Replace(" isNavigationBarHidden", "NavigationBarHidden")
                .Replace(" navigationBar", ".NavigationBar")
                .Replace("[UIApplication sharedApplication]", "UIApplication.SharedApplication")
                .Replace("UIInterfaceOrientationIsPortrait(", "UIInterfaceOrientation.IsPortrait(")
                .Replace(" statusBarOrientation", ".StatusBarOrientation")
                .Replace(".view", ".View")
                .Replace("[[UILabel alloc] init]", "new UILabel()")
                .Replace(" setText:", ".Text = ")
                .Replace("UIControlStateNormal", "UIControlState.Normal")
                .Replace(" setBackgroundImage:", ".SetBackgroundImage(")
                .Replace(" setTitle:", ".SetTitle(")
                .Replace(" setTitleColor:", ".SetTitleColor(")
                .Replace(" setTextColor:", ".TextColor = ")
                .Replace(" setBackgroundColor:", ".BackgroundColor = ")
                .Replace(".contentEdgeInsets", ".ContentEdgeInsets")
                .Replace(" sizeToFit]", ".SizeToFit()")
                .Replace("[[UIView alloc] initWithFrame:", "new UIView(")
                .Replace("UISwipeGestureRecognizerDirectionUp", "UISwipeGestureRecognizerDirection.Up")
                .Replace("UISwipeGestureRecognizerDirectionDown", "UISwipeGestureRecognizerDirection.Down")
                .Replace("UISwipeGestureRecognizerDirectionLeft", "UISwipeGestureRecognizerDirection.Left")
                .Replace("UISwipeGestureRecognizerDirectionRight", "UISwipeGestureRecognizerDirection.Right")
                .Replace("[super layoutSubviews];", "base.LayoutSubviews();")
                .Replace(" setFont:", ".Font = ")
                .Replace(" setShadowColor:", ".ShadowColor = ")
                .Replace(" setShadowOffset:", ".ShadowOffset = ")
                .Replace("[UIFont boldSystemFontOfSize:", "UIFont.BoldSystemFontOfSize(")
                .Replace("[UIFont systemFontOfSize:", "UIFont.SystemFontOfSize(")
                .Replace(".numberOfLines", ".Lines")
                .Replace(".lineBreakMode", ".LineBreakMode")
                .Replace("NSLineBreakByWordWrapping", "UILineBreakMode.WordWrap")
                .Replace(" addSubview:", ".Add(")
                .Replace("UIGraphicsBeginImageContextWithOptions", "UIGraphics.BeginImageContextWithOptions")
                .Replace("UIGraphicsEndImageContext", "UIGraphics.EndImageContext")
                .Replace("UIGraphicsGetImageFromCurrentImageContext", "UIGraphics.GetImageFromCurrentImageContext")
                .Replace("UIGraphicsGetCurrentContext", "UIGraphics.GetCurrentContext")
                .Replace("CGContextRef", "CGContext")
                .Replace("CGPathCreateMutable()", "new CGPath()")
                .Replace("CGMutablePathRef", "CGPath")
                .Replace("kCGGradientDrawsAfterEndLocation", "CGGradientDrawingOptions.DrawsAfterEndLocation")
                .Replace("kCGGradientDrawsBeforeStartLocation", "CGGradientDrawingOptions.DrawsBeforeStartLocation")
                .Replace("NULL", "null")
                .Replace(".statusBarFrame", ".StatusBarFrame")
                .Replace("UIViewAutoresizingFlexibleWidth", "UIViewAutoresizing.FlexibleWidth")
                .Replace("UIViewAutoresizingFlexibleHeight", "UIViewAutoresizing.FlexibleHeight")
                .Replace("UIViewAutoresizingFlexibleTopMargin", "UIViewAutoresizing.FlexibleTopMargin")
                .Replace("UIViewAutoresizingFlexibleBottomMargin", "UIViewAutoresizing.FlexibleBottomMargin")
                .Replace(" count]", ".Count")
                .Replace("[UIImage imageNamed:", "UIImage.FromName(")
                .Replace("[[UIImageView alloc] initWithImage:", "new UIImageView(")
                .Replace(" objectAtIndex:", "[")
                .Replace("[[NSBundle mainBundle] resourcePath]", "NSBundle.MainBundle.ResourcePath")
                .Replace("NSString", "string")
                .Replace(".viewController", ".ViewController")
                .Replace("BOOL ", " bool ")
                .Replace("(BOOL)", "bool ")
                .Replace("completion:^", ", delegate")
                .Replace("[UIView animateWithDuration:", "UIView.Animate(")
                .Replace("animations:^", ", delegate {")
                .Replace(".center", ".Center")
                .Replace("[UIButton buttonWithType:UIButtonTypeCustom]", "UIButton.FromType(UIButtonType.Custom)")
                .Replace(".tag", ".Tag")
                .Replace(".clipsToBounds", ".ClipsToBounds")
                .Replace(".contentMode", ".ContentMode")
                .Replace(".text", ".Text")
                .Replace(".font", ".Font")
                .Replace(".enabled", ".Enabled")
                .Replace("CGRectZero", "RectangleF.Empty")
                .Replace("UIViewAutoresizingNone", "UIViewAutoresizing.None")
                .Replace("NSUInteger", "int")
                .Replace("[self ", "this.")
                .Replace("[self.", "this.")
                .Replace(" removeFromSuperview", ".RemoveFromSuperview")
                .Replace("MAX(", "Math.Max(")
                .Replace("MIN(", "Math.Min(")
                .Replace("CGColorSpaceRef", "CGColorSpace")
                .Replace("CGColorSpaceCreateDeviceRGB", "CGColorSpace.CreateDeviceRGB")
                .Replace("CGGradientRef", "CGGradient")
                .Replace("CGGradientCreateWithColorComponents", "new CGGradient")
                .Replace(".autoresizingMask", ".AutoresizingMask")
                .Replace(".backgroundColor", ".BackgroundColor")
                .Replace(".opaque", ".Opaque")
                .Replace(" NO", " false")
                .Replace(" YES", " true")
                .Replace(".image.size", ".Image.Size")
                .Replace("[UIColor clearColor]", "UIColor.Clear")
                .Replace("CGFloat", "float")
                .Replace(".size.width", ".Width")
                .Replace(".size.height", ".Height")
                .Replace(".width", ".Width")
                .Replace(".height", ".Height")
                .Replace(".origin.x", ".X")
                .Replace(".origin.y", ".Y")
                .Replace(".x", ".X")
                .Replace(".y", ".Y")
                .Replace(".frame", ".Frame")
                .Replace(".bounds", ".Bounds")
                .Replace(".layer", ".Layer")
                .Replace(".shadowOpacity", ".ShadowOpacity")
                .Replace(".shadowOffset", ".ShadowOffset")
                .Replace(".shadowColor", ".ShadowColor")
                .Replace(".shadowRadius", ".ShadowRadius")
                .Replace("CGSizeMake", "new SizeF")
                .Replace("CGSize", "SizeF")
                .Replace("CGRect", "RectangleF")
                .Replace("CGPoint", "PointF")
                .Replace(".alpha", ".Alpha")
                ;
            return text;
        }

        private static string ReplaceDefineStatements(string text)
        {
            var defineEvaluator = new MatchEvaluator(match =>
            {
                string variable = match.Groups[1].Value;
                string value = match.Groups[2].Value;

                string suffix = "";
                string type = "";
                if (value.StartsWith("@\""))
                {
                    value = value.Substring(1);
                    type = "string";
                }
                else
                {
                    suffix = "f";
                    type = "float";
                }

                return string.Format("public const {0} {1} = {2}{3};", type, variable, value);
            });

            text = Regex.Replace(text, @"^#define ([a-zA-Z]*?) (.*?)$", defineEvaluator, RegexOptions.Multiline);
            return text;
        }
    }
}

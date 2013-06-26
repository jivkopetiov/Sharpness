using Sharpness;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sharpness
{
    public class SharpnessParser
    {
        public string Parse(string input)
        {
            return ParseImpl(input);
        }

        public void ParseFile(string inputFile, string outputDirectory)
        {
            string parsed = ParseImpl(File.ReadAllText(inputFile));
            string outputFile = Path.Combine(outputDirectory, new FileInfo(inputFile).Name);
            File.WriteAllText(outputFile, parsed);
        }

        private string ParseImpl(string text)
        {
            text = ReplaceDefineStatements(text);

            // two parameter function
            SmartReplace(ref text, @"- \( <var1> \*?\) <var2>:\(<var3> \*?\) <var4> <var5>:\(<var6> \*?\) <var7> \{", match =>
            {
                string param1 = match.Groups[1].Value;
                string param2 = match.Groups[2].Value;
                string param3 = match.Groups[3].Value;
                string param4 = match.Groups[4].Value;
                string param5 = match.Groups[5].Value;
                string param6 = match.Groups[6].Value;
                string param7 = match.Groups[7].Value;

                if (param1 == "id" && param2 == "initWithFrame" && param3 == "CGRect")
                    return string.Format("private {0}(RectangleF {1}, {2} {3}) {{", ClassName, param4, param6, param7);

                else if (param2 == "tableView" && param5 == "numberOfRowsInSection")
                    return string.Format("private {0} {1}({2} {3}, {4} {5}) {{", param1, "RowsInSection", param3, param4, param6, param7);
                else if (param2 == "tableView" && param5 == "cellForRowAtIndexPath")
                    return string.Format("private {0} {1}({2} {3}, {4} {5}) {{", param1, "GetCell", param3, param4, param6, param7);
                else if (param2 == "tableView" && param5 == "viewForHeaderInSection")
                    return string.Format("private {0} {1}({2} {3}, {4} {5}) {{", param1, "ViewForHeader", param3, param4, param6, param7);
                else if (param2 == "tableView" && param5 == "heightForRowAtIndexPath")
                    return string.Format("private {0} {1}({2} {3}, {4} {5}) {{", param1, "HeightForRow", param3, param4, param6, param7);
                else if (param2 == "tableView" && param5 == "didSelectRowAtIndexPath")
                    return string.Format("private {0} {1}({2} {3}, {4} {5}) {{", param1, "RowSelected", param3, param4, param6, param7);

                else
                    return string.Format("private {0} {1}({2} {3}, {4} {5}) {{", param1, param2, param3, param4, param6, param7);
            });




            //- (void) _movePagesToIndex:(NSInteger)nowPage animated:(bool)animated{

            // one parameter function
            SmartReplace(ref text, @"- \( <var1> \*?\) <var2>:\(<var3> \*?\) <var4> \{", match =>
            {
                string param1 = match.Groups[1].Value;
                string param2 = match.Groups[2].Value;
                string param3 = match.Groups[3].Value;
                string param4 = match.Groups[4].Value;

                if (param1 == "id" && param2 == "initWithFrame" && param3 == "CGRect")
                    return string.Format("private {0}(RectangleF {1}) {{", ClassName, param4);
                else if (param1 == "id" && param2 == "initWithCoder" && param3 == "NSCoder")
                    return string.Format("private {0}(NSCoder {1}) {{", ClassName, param4);
                else
                    return string.Format("private {0} {1}({2} {3}) {{", param1, param2, param3, param4);
            });

            // zero parameter function
            SmartReplace(ref text, @"- \( <var1> \*?\) <var2> \{", match =>
            {
                string param1 = match.Groups[1].Value;
                string param2 = match.Groups[2].Value;

                if (param1 == "id" && param2 == "init")
                    return string.Format("private {0}() {{", ClassName);
                else
                    return string.Format("private {0} {1}() {{", param1, param2);
            });

            text = text.Replace("NSString", "string");

            // UILabel *label; -- remove star
            SmartReplace(ref text, @"^( <var> )\*(<var>; )$", "$1$2", RegexOptions.Multiline);

            // [view addSubview:hud]; -> view.AddSubview(hud);
            SmartReplace(ref text, @"\[<var1> addSubview:<var2>\];", "$1.Add($2);");

            // [hud show:animated]; -> hud.show(animated);
            SmartReplace(ref text, @"\[<var1> <var2>:<var3>\];", "$1.$2($3);");

            // [this setupLabels]; -> this.setupLabels();
            SmartReplace(ref text, @"\[<var1> <var2>\];", "$1.$2();");

            // CGRectInset(allRect, 2.0f, 2.0f); -> allRect.Inset(2.0f, 2.0f);
            SmartReplace(ref text, @"CGRectInset\(<varNum1>,", "$1.Inset(");

            // [[UIColor alloc] initWithWhite:1.f alpha:1.f] -> UIColor.FromWhiteAlpha(1.f, 1.f)
            SmartReplace(ref text, @"\[\[UIColor  alloc\] initWithWhite:([\d\.f]*?) alpha:([\d\.f]*?)\]", "UIColor.FromWhiteAlpha($1, $2)");

            //[UIColor colorWithWhite:0 alpha:0.8]
            SmartReplace(ref text, @"\[ UIColor  colorWithWhite: <varNum1>  alpha: <varNum2> \]", "UIColor.FromWhiteAlpha($1, $2)");

            // [keyPath isEqualToString:@"labelText"] -> keyPath == "labelText"
            SmartReplace(ref text, @"\[<var1> isEqualToString:@\""<var2>\""\]", @"$1 == ""$2""");

            // [label.Text sizeWithFont:label.Font] -> new NSString(label.Text).StringSize(label.Font)
            text = Regex.Replace(text, @"\[([_a-zA-Z\.]*?)\s*sizeWithFont:([_a-zA-Z\.]*?)\]", "new NSString($1).StringSize($2)");

            SmartReplace(ref text, @"\[\[UILabel  alloc\] init\]", "new UILabel()");
            text = Regex.Replace(text, @"\[\[UILabel alloc\] initWithFrame:([\._a-zA-Z]*?)\]", "new UILabel($1)");

            // UIFont
            SmartReplace(ref text, @"\[ UIFont  boldSystemFontOfSize : <varNum1> \]", "UIFont.BoldSystemFontOfSize($1)");
            SmartReplace(ref text, @"\[ UIFont  systemFontOfSize : <varNum1> \]", "UIFont.SystemFontOfSize($1)");

            //UIImageView *iv = [[UIImageView alloc] initWithFrame:CGRectMake(0,0, NOB_SIZE, NOB_SIZE)];
            SmartReplace(ref text, @"UIImageView  \*<var1> = \[\[ UIImageView  alloc \]  initWithFrame : ([^\]]*?) \];", "UIImageView $1 = new UIImageView($2);");

            // word replacements, nill, self, YES, NO
            text = Regex.Replace(text, @"\bnil\b", "null");
            text = Regex.Replace(text, @"\bself\b", "this");
            text = Regex.Replace(text, @"\bYES\b", "true");
            text = Regex.Replace(text, @"\bNO\b", "false");
            text = Regex.Replace(text, @"\bBOOL\b", "bool");
            text = Regex.Replace(text, @"\bNSInteger\b", "int");

            text = Regex.Replace(text, @"\bUIInterfaceOrientationPortraitUpsideDown\b", "UIInterfaceOrientation.PortraitUpsideDown");
            text = Regex.Replace(text, @"\bUIInterfaceOrientationPortrait\b", "UIInterfaceOrientation.Portrait");
            text = Regex.Replace(text, @"\bUIInterfaceOrientationLandscapeLeft\b", "UIInterfaceOrientation.LandscapeLeft");
            text = Regex.Replace(text, @"\bUIInterfaceOrientationLandscapeRight\b", "UIInterfaceOrientation.LandscapeRight");
            text = Regex.Replace(text, @"\bUIInterfaceOrientationIsLandscape\b", "UIInterfaceOrientation.IsLandscape");
            text = Regex.Replace(text, @"\bUIInterfaceOrientationIsPortrait\b", "UIInterfaceOrientation.IsPortrait");

            var capitalizeProperties = new[] { 
                "setNeedsDisplay",
                "setStroke",
                "setFill",
                "set",
                "stroke",
                "sizeToFit",
                "lineWidth",
                "lineCapStyle",
                "frame",
                "bounds",
                "layer",
                "shadowOpacity",
                "shadowOffset",
                "shadowColor",
                "shadowRadius",
                "shadowPath",
                "removeFromSuperview"
            };

            foreach (string prop in capitalizeProperties)
                //text = Regex.Replace(text, @"\b" + word + @"\b", word[0].ToString().ToUpperInvariant() + word.Substring(1));
                text = text.Replace("." + prop, "." + prop[0].ToString().ToUpperInvariant() + prop.Substring(1));

            SmartReplace(ref text, @"^@property (\([^)]*?\)) <var1> \*?([^\*]*?);$", "public $2 $3;", RegexOptions.Multiline);

            text = Regex.Replace(text, @"/\*[\w\W]*?\*/", "");

            text = Regex.Replace(text, "^@implementation(.*?)$", "public class$1", RegexOptions.Multiline);
            text = text.Replace("@end", "}");
            text = Regex.Replace(text, @"\(CGPoint\)\d*?{([\w\W]*?)}", "new PointF($1)");
            text = Regex.Replace(text, @"\(CGRect\)\d*?{([\w\W]*?)}", "new RectangleF($1)");

            text = Regex.Replace(text, @"CGRectGetMinX\((.*?)\)", "$1.GetMinX()");
            text = Regex.Replace(text, @"CGRectGetMaxX\((.*?)\)", "$1.GetMaxX()");
            text = Regex.Replace(text, @"CGRectGetMinY\((.*?)\)", "$1.GetMinY()");
            text = Regex.Replace(text, @"CGRectGetMaxY\((.*?)\)", "$1.GetMaxY()");
            text = Regex.Replace(text, @"CGRectGetMidX\((.*?)\)", "$1.GetMidX()");
            text = Regex.Replace(text, @"CGRectGetMidY\((.*?)\)", "$1.GetMidY()");
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
            text = Regex.Replace(text, @"CGContextDrawRadialGradient\s?\(([^,]*?),", "$1.DrawRadialGradient(");
            text = Regex.Replace(text, @"CGContextSetLineWidth\s?\(([^,]*?),", "$1.SetLineWidth(");
            text = Regex.Replace(text, @"CGContextFillEllipseInRect\s?\(([^,]*?),", "$1.FillEllipseInRect(");
            text = Regex.Replace(text, @"CGContextStrokeEllipseInRect\s?\(([^,]*?),", "$1.StrokeEllipseInRect(");
            text = Regex.Replace(text, @"CGContextMoveToPoint\s?\(([^,]*?),", "$1.MoveToPoint(");
            text = Regex.Replace(text, @"CGContextAddArcToPoint\s?\(([^,]*?),", "$1.AddArcToPoint(");
            text = Regex.Replace(text, @"CGContextDrawPath\s?\(([^,]*?),", "$1.DrawPath(");
            text = Regex.Replace(text, @"CGContextAddArc\s?\(([^,]*?),", "$1.AddArc(");
            text = Regex.Replace(text, @"CGContextSetFillColorWithColor\s?\(([^,]*?),", "$1.SetFillColorWithColor(");
            text = Regex.Replace(text, @"CGContextSetGrayFillColor\s?\(([^,]*?),", "$1.SetGrayFillColor(");


            text = Regex.Replace(text, @"CGContextFillPath\(([^\)]*?)\);", "$1.FillPath();");
            text = Regex.Replace(text, @"CGContextClosePath\(([^\)]*?)\);", "$1.ClosePath();");
            text = Regex.Replace(text, @"CGContextSaveGState\(([^\)]*?)\);", "$1.SaveState();");
            text = Regex.Replace(text, @"CGContextRestoreGState\(([^\)]*?)\);", "$1.RestoreState();");
            text = Regex.Replace(text, @"CGContextClip\(([^\)]*?)\);", "$1.Clip();");
            text = Regex.Replace(text, @"(CGPathRelease|CGGradientRelease|CGColorSpaceRelease)\(([^\)]*?)\);", "$2.Dispose();");
            text = Regex.Replace(text, @"UIGraphicsPushContext\(([^\)]*?)\);", "$1.Push();");

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
            text = text
                .Replace("roundf", "Math.Round")
                .Replace("PointFZero", "PointF.Empty")
                .Replace("CGSizeZero", "SizeF.Empty")
                .Replace("CGPointMake", "new PointF")
                .Replace("CGRectMake", "new RectangleF")
                .Replace("UIEdgeInsetsMake", "new UIEdgeInsets")
                .Replace(" isNavigationBarHidden", "NavigationBarHidden")
                .Replace(" navigationBar", ".NavigationBar")
                .Replace("[UIApplication sharedApplication]", "UIApplication.SharedApplication")
                .Replace(" statusBarOrientation", ".StatusBarOrientation")
                .Replace(".statusBarOrientation", ".StatusBarOrientation")
                .Replace("this.superview", "this.Superview")
                .Replace(" setText:", ".Text = ")
                .Replace(".boldSystemFontOfSize", ".BoldSystemFontOfSize")
                .Replace(".systemFontOfSize", ".SystemFontOfSize")
                .Replace("UIControlStateNormal", "UIControlState.Normal")
                .Replace(" setBackgroundImage:", ".SetBackgroundImage(")
                .Replace(" setTitle:", ".SetTitle(")
                .Replace(" setTitleColor:", ".SetTitleColor(")
                .Replace(" setTextColor:", ".TextColor = ")
                .Replace(" setBackgroundColor:", ".BackgroundColor = ")
                .Replace(".contentEdgeInsets", ".ContentEdgeInsets")
                .Replace("[[UIView alloc] initWithFrame:", "new UIView(")
                .Replace("[UIScreen mainScreen]", "UIScreen.MainScreen")
                .Replace("UISwipeGestureRecognizerDirectionUp", "UISwipeGestureRecognizerDirection.Up")
                .Replace("UISwipeGestureRecognizerDirectionDown", "UISwipeGestureRecognizerDirection.Down")
                .Replace("UISwipeGestureRecognizerDirectionLeft", "UISwipeGestureRecognizerDirection.Left")
                .Replace("UISwipeGestureRecognizerDirectionRight", "UISwipeGestureRecognizerDirection.Right")
                .Replace("[super layoutSubviews];", "base.LayoutSubviews();")
                .Replace("NSTextAlignmentCenter", "NSTextAlignment.Center")
                .Replace(" setFont:", ".Font = ")
                .Replace(" setShadowColor:", ".ShadowColor = ")
                .Replace(" setShadowOffset:", ".ShadowOffset = ")
                .Replace(".numberOfLines", ".Lines")
                .Replace(".lineBreakMode", ".LineBreakMode")
                .Replace("NSLineBreakByWordWrapping", "UILineBreakMode.WordWrap")
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
                .Replace("UIViewAutoresizingFlexibleLeftMargin", "UIViewAutoresizing.FlexibleLeftMargin")
                .Replace("UIViewAutoresizingFlexibleRightMargin", "UIViewAutoresizing.FlexibleRightMargin")
                .Replace("CGAffineTransformIdentity", "CGAffineTransform.MakeIdentity")
                .Replace("CGAffineTransformIdentity", "CGAffineTransform.MakeIdentity")
                .Replace("CGAffineTransformScale", "CGAffineTransform.Scale")
                .Replace("CGAffineTransformMakeRotation", "CGAffineTransform.MakeRotation")
                .Replace("[UIImage imageNamed:", "UIImage.FromName(")
                .Replace("[[UIImageView alloc] initWithImage:", "new UIImageView(")
                .Replace(" objectAtIndex:", "[")
                .Replace("[[NSBundle mainBundle] resourcePath]", "NSBundle.MainBundle.ResourcePath")
                .Replace(".viewController", ".ViewController")
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
                .Replace(" removeFromSuperview", ".RemoveFromSuperview")
                .Replace("MAX(", "Math.Max(")
                .Replace("MIN(", "Math.Min(")
                .Replace("M_PI", "Math.PI")
                .Replace("CGColorSpaceRef", "CGColorSpace")
                .Replace("CGColorSpaceCreateDeviceRGB", "CGColorSpace.CreateDeviceRGB")
                .Replace("CGGradientRef", "CGGradient")
                .Replace("CGGradientCreateWithColorComponents", "new CGGradient")
                .Replace(".autoresizingMask", ".AutoresizingMask")
                .Replace(".backgroundColor", ".BackgroundColor")
                .Replace(".opaque", ".Opaque")
                .Replace(".image.size", ".Image.Size")
                .Replace("UIColor.clearColor()", "UIColor.Clear")
                .Replace("UIColor.whiteColor()", "UIColor.White")
                .Replace("CGFloat", "float")
                .Replace(".size.width", ".Width")
                .Replace(".size.height", ".Height")
                .Replace(".width", ".Width")
                .Replace(".height", ".Height")
                .Replace(".origin.x", ".X")
                .Replace(".origin.y", ".Y")
                .Replace(".x", ".X")
                .Replace(".y", ".Y")
                .Replace("CGSizeMake", "new SizeF")
                .Replace("CGSize", "SizeF")
                .Replace("CGRect", "RectangleF")
                .Replace("CGPoint", "PointF")
                .Replace(".alpha", ".Alpha")
                ;

            return text;
        }

        private Metadata TransformFile(string filePath, string outputDir, Metadata metadata)
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

        private void ParseHeaderFile(FileInfo m, string filename, Metadata metadata)
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

        private string TransformM(string text, Metadata metadata)
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

            text = ParseImpl(text);

            return text;
        }

        private static void SmartReplace(ref string text, string pattern, string replacement)
        {
            SmartReplace(ref text, pattern, replacement, RegexOptions.None);
        }

        private static void SmartReplace(ref string text, string pattern, string replacement, RegexOptions options)
        {
            text = Regex.Replace(text, NormalizeRegex(pattern), replacement, options);
        }

        private static void SmartReplace(ref string text, string pattern, MatchEvaluator evaluator)
        {
            SmartReplace(ref text, pattern, evaluator, RegexOptions.None);
        }

        private static void SmartReplace(ref string text, string pattern, MatchEvaluator evaluator, RegexOptions options)
        {
            text = Regex.Replace(text, NormalizeRegex(pattern), evaluator, options);
        }

        private static string NormalizeRegex(string regex)
        {
            string varIdentifier = @"[0-9a-zA-Z_]*?";
            string varNumIdentifier = @"[0-9a-zA-Z_\./]*?";

            regex = regex.Replace("  ", @"\s+")
                        .Replace(" ", @"\s*")
                        .Replace("<var>", varIdentifier)
                        .Replace("<varNum>", varIdentifier);

            regex = Regex.Replace(regex, @"<var\d{1,2}>", "(" + varIdentifier + ")");
            regex = Regex.Replace(regex, @"<varNum\d{1,2}>", "(" + varNumIdentifier + ")");

            return regex;
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
                    // is certainly float 
                    if (Regex.IsMatch(value, @"^[\.\df]*?$"))
                    {
                        if (!value.EndsWithOrdinalNoCase("f"))
                            suffix = "f";
                    }

                    type = "float";
                }

                return string.Format("public const {0} {1} = {2}{3};", type, variable, value, suffix);
            });

            text = Regex.Replace(text, @"^#define ([_a-zA-Z]*?) (.*?)$", defineEvaluator, RegexOptions.Multiline);
            return text;
        }

        public string ClassName { get; set; }
    }
}

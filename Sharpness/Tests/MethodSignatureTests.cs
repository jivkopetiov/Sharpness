using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharpness.Tests
{
    [TestFixture]
    public class MethodSignatureTests
    {
        [TestCase("- (void) _setupView{", Result = "private void _setupView() {")]
        [TestCase("     -(void) didMoveToWindow {", Result = "private void didMoveToWindow() {")]
        [TestCase("- (void)viewDidLoad {", Result = "private void viewDidLoad() {")]
        public string NoParams(string input)
        {
            return new SharpnessParser().Parse(input);
        }

        [TestCase("- (void) _movePagesToIndex:(NSInteger)nowPage animated:(BOOL)animated{",
            Result = "private void _movePagesToIndex(int nowPage, bool animated) {")]
        [TestCase("-(void)updateForPinchScale:(CGFloat)scale atIndexPath:(NSIndexPath*)indexPath {",
            Result = "private void updateForPinchScale(float scale, NSIndexPath indexPath) {")]
        public string TwoParams(string input)
        {
            return new SharpnessParser().Parse(input);
        }


        [TestCase("- (void) _scrollToTopEvent:(BOOL)animated{", Result = "private void _scrollToTopEvent(bool animated) {")]
        [TestCase("- (TKTimelineView*) _timelineWithScrollView:(UIScrollView*)sv{", Result = "private TKTimelineView _timelineWithScrollView(UIScrollView sv) {")]
        public string OneParam(string input)
        {
            return new SharpnessParser().Parse(input);
        }

        [TestCase("- (id) initWithFrame:(CGRect)frame{", Result = "private ClassName(RectangleF frame) {")]
        [TestCase("- (id) initWithCoder:(NSCoder *)decoder{", Result = "private ClassName(NSCoder decoder) {")]
        [TestCase("- (id) init{", Result = "private ClassName() {")]
        [TestCase("- (id) initWithFrame:(CGRect)frame timeZone:(NSTimeZone*)timeZone{", Result = "private ClassName(RectangleF frame, NSTimeZone timeZone) {")]
        public string Constructors(string input)
        {
            return new SharpnessParser() { ClassName = "ClassName" }.Parse(input);
        }

        [TestCase("-(NSInteger)tableView:(UITableView*)tableView numberOfRowsInSection:(NSInteger)section {",
            Result = "private int RowsInSection(UITableView tableView, int section) {")]
        [TestCase("-(UITableViewCell*)tableView:(UITableView*)tableView cellForRowAtIndexPath:(NSIndexPath*)indexPath {",
            Result = "private UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath) {")]
        [TestCase("-(UIView*)tableView:(UITableView*)tableView viewForHeaderInSection:(NSInteger)section {",
            Result = "private UIView ViewForHeader(UITableView tableView, int section) {")]
        [TestCase("-(CGFloat)tableView:(UITableView*)tableView heightForRowAtIndexPath:(NSIndexPath*)indexPath {",
            Result = "private float HeightForRow(UITableView tableView, NSIndexPath indexPath) {")]
        [TestCase("-(void)tableView:(UITableView*)tableView didSelectRowAtIndexPath:(NSIndexPath*)indexPath {",
            Result = "private void RowSelected(UITableView tableView, NSIndexPath indexPath) {")]
        public string UITableViewMethods(string input)
        {
            return new SharpnessParser().Parse(input);
        }

        [TestCase("[UIColor colorWithWhite:0 alpha:0.8]", Result = "UIColor.FromWhiteAlpha(0, 0.8)")]
        public string UIColor(string input)
        {
            return new SharpnessParser().Parse(input);
        }

        [TestCase("[UIFont boldSystemFontOfSize:14]", Result = "UIFont.BoldSystemFontOfSize(14)")]
        [TestCase("[UIFont systemFontOfSize:fontSize]", Result = "UIFont.SystemFontOfSize(fontSize)")]
        public string UIFont(string input)
        {
            return new SharpnessParser().Parse(input);
        }
        
    }
}

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
        // TODO
        //- (void) scrollViewWillEndDragging:(UIScrollView *)scrollView withVelocity:(CGPoint)velocityTangent targetContentOffset:(inout CGPoint *)targetContentOffset{


        [TestCase("- (void) _setupView{", Result = "private void _setupView() {")]
        [TestCase("     -(void) didMoveToWindow {", Result = "private void didMoveToWindow() {")]
        [TestCase("- (void)viewDidLoad {", Result = "private void viewDidLoad() {")]
        public string NoParams(string input)
        {
            return new SharpnessParser().Parse(input);
        }

        [TestCase("- (void) _scrollToTopEvent:(BOOL)animated{", Result = "private void _scrollToTopEvent(bool animated) {")]
        [TestCase("- (TKTimelineView*) _timelineWithScrollView:(UIScrollView*)sv{", Result = "private TKTimelineView _timelineWithScrollView(UIScrollView sv) {")]
        [TestCase("- (void) setCoverflowDataSource:(id<TKCoverflowViewDataSource>)coverflowDataSource{",
            Result = "private void setCoverflowDataSource(TKCoverflowViewDataSource coverflowDataSource) {")]
        public string OneParam(string input)
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

        [TestCase("- (UIImage*) imageForKey:(string*)key url:(NSURL*)url queueIfNeeded:(bool)queueIfNeeded{",
            Result = "private UIImage imageForKey(string key, NSUrl url, bool queueIfNeeded) {")]
        public string ThreeParams(string input)
        {
            return new SharpnessParser().Parse(input);
        }

        [TestCase("- (UIImage*) imageForKey:(string*)key url:(NSUrl*)url queueIfNeeded:(bool)queueIfNeeded tag:(int)tag{",
            Result = "private UIImage imageForKey(string key, NSUrl url, bool queueIfNeeded, int tag) {")]
        public string FourParams(string input)
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
    }
}

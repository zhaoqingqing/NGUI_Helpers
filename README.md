# NGUI_Helpers
对NGUI的某些组件进行封装，提高易用性。
#建议版本
Unity 5.x

NGUI 3.7 及以上版本
#组件列表
##UIWrapContentHelper
此组件的使用方法及原理，欢迎查看我的博客：

 UIWrapContent(NGUI长列表优化利器) [http://www.cnblogs.com/zhaoqingqing/p/4901393.html](http://www.cnblogs.com/zhaoqingqing/p/4901393.html)
##UIPanelResetHelper
如果我们的UI中有滑动列表，并且列表比较长，那么不知道你们是否有这样需求，每次页面打开时，列表的滑动状态都恢复到默认状态。

如果要复位，其实就是修改UIPanel 的属性到初始状态。此组件做的工作就是在初始化时把UIPanel的属性保存起来，在需要时还原初始值，达到复位效果。

UIPanelResetHelper(UIScrollView滚动复位) [http://www.cnblogs.com/zhaoqingqing/p/5546927.html](http://www.cnblogs.com/zhaoqingqing/p/5546927.html "UIPanelResetHelper(UIScrollView滚动复位)")
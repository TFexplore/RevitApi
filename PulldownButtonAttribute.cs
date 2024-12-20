using System;

namespace LiangChi.MepDesign.Revit.Ribbons
{
    /// <summary>
    /// 自定义特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class PulldownButtonAttribute : ButtonAttribute
    {
        /// <summary>
        /// 常规
        /// </summary>
        /// <param name="name">控件名称</param>
        /// <param name="text">显示值</param>
        /// <param name="image">显示图片路径</param>
        /// <param name="largeImage">以大图显示的路径</param>
        public PulldownButtonAttribute( string name, string text, string image = null, string largeImage = null) : base(text, image, largeImage)
        {
            this.Name = name;
        }

    }


}

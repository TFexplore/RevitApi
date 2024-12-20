using System;

namespace LiangChi.MepDesign.Revit.Ribbons
{
    /// <summary>
    /// 自定义特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class PushButtonAttribute : ButtonAttribute
    {
        /// <summary>
        /// 常规
        /// </summary>
        /// <param name="text">显示值</param>
        /// <param name="image">显示图片路径</param>
        /// <param name="largeImage">以大图显示的路径</param>
        public PushButtonAttribute(string text, string image = null, string largeImage = null) : base(text, image, largeImage)
        {

        }
        /// <summary>
        /// 关联的命令类
        /// </summary>
        public Type CommandClass { get; set; }
    }


}

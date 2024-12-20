using System;

namespace LiangChi.MepDesign.Revit.Ribbons
{
    /// <summary>
    /// 自定义特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ButtonAttribute : RibbonItemAttribute
    {
        /// <summary>
        /// 常规
        /// </summary>
        /// <param name="text">显示值</param>
        /// <param name="image">显示图片路径</param>
        /// <param name="largeImage">以大图显示的路径</param>
        public ButtonAttribute(string text, string image = null, string largeImage = null)
        {
            this.Text = text;
            this.LargeImage = largeImage;
            this.Image = image;
            if (string.IsNullOrEmpty(this.Text))
            {
                this.Text = "未命名按钮";
            }
        }
        /// <summary>
        /// 显示文本
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// 显示（大）图片路径
        /// </summary>
        public string LargeImage { get; set; }

        /// <summary>
        /// 显示图片路径
        /// </summary>
        public string Image { get; set; }

        public bool IsAlwaysOk { get; set; } = false;


    }


}

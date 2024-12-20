using System;

namespace LiangChi.MepDesign.Revit.Ribbons
{
    /// <summary>
    /// 自定义特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RibbonItemAttribute : Attribute
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; } 

        /// <summary>
        /// 提示文本
        /// </summary>
        public string ToolTip { get; set; }

        /// <summary>
        /// 提示图片路径
        /// </summary>
        public string ToolTipImage { get; set; }

        /// <summary>
        /// 是否前置分割符
        /// </summary>
        public bool Separator { get; set; }

        /// <summary>
        /// 是否前置滑块
        /// </summary>
        public bool SlideOut { get; set; }

        /// <summary>
        /// 行号 至多3
        /// </summary>
        public int RowIndex { get; set; }

        /// <summary>
        /// 列号，根据列号从左到右
        /// </summary>
        public int ColumnIndex { get; set; }



    }


}

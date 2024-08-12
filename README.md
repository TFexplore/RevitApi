三维物体检测：
 var linkDoc = LinkQueryUtil.QueryLinkedDocuments(doc).FirstOrDefault();
 // 查找文档中的3D视图
 FilteredElementCollector collector = new FilteredElementCollector(linkDoc);
 var view3D = collector.OfClass(typeof(View3D))
                         .Cast<View3D>().Where(v => !v.Name.Contains("副本"))
                         .FirstOrDefault(v => !v.IsTemplate);
 FilteredElementCollector cltora = new FilteredElementCollector(linkDoc, view3D.Id).WherePasses(new ElementMulticategoryFilter(new[] { BuiltInCategory.OST_StructuralFraming, BuiltInCategory.OST_Walls, BuiltInCategory.OST_StructuralColumns, BuiltInCategory.OST_Columns }))
     .WhereElementIsNotElementType();
 var intersector = new ReferenceIntersector(cltora.ToElementIds(), FindReferenceTarget.All, view3D);
 intersector.FindReferencesInRevitLinks = true;
 var referenceWithContext = intersector.FindNearest(item.EndPoint, d);        
  Reference reference = referenceWithContext.GetReference();
 
 Element element = linkDoc.GetElement(reference);

 XYZ point = reference.GlobalPoint;
       
        
        
        /// <summary>
        /// 查询当前文档中的所有链接文档
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static List<Document> QueryLinkedDocuments(Document doc)
        {
            List<Document> result = new List<Document>();
            // 获取所有的链接文件
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(RevitLinkInstance));
            var linkInstances = collector.ToElements().Cast<RevitLinkInstance>().ToList();
            foreach (var item in linkInstances)
            {
                // 获取链接文件的文档
                Document linkDoc = item.GetLinkDocument();
                result.Add(linkDoc);
            }
            return result;
        }

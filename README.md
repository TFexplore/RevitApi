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

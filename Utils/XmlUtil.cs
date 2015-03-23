using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace hAutobot.Utils
{
    public class XmlUtil
    {
        private XmlDocument xmlDoc = null;
        private string docPath = "";

        #region Creator
        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="path">읽어올 xml 파일 경로</param>
        public XmlUtil(string path)
        {
            try
            {
                xmlDoc = new XmlDocument();
                xmlDoc.Load(path);
                docPath = path;
            }
            catch (Exception ex)
            {
                xmlDoc = null;
                Console.WriteLine(ex.Message);
            }
        }
        #endregion

        #region Node의 값을 읽어온다.
        /// <summary>
        /// path에 해당하는 node의 값을 가져온다.
        /// </summary>
        /// <param name="xpath"></param>
        /// <returns></returns>
        public string GetNodeValue(string xpath)
        {
            if (xmlDoc == null) return "";

            string value = "";

            try
            {
                XmlNodeList list = xmlDoc.DocumentElement.SelectNodes(xpath);

                if (list.Count > 0)
                    value = list[0].InnerText;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                value = string.Empty;
            }

            return value;
        }

        public string[] GetNodeValues(string xpath)
        {
            if (xmlDoc == null) return null;

            List<string> value = new List<string>();

            try
            {
                XmlNodeList list = xmlDoc.DocumentElement.SelectNodes(xpath);

                foreach (XmlNode node in list)
                {
                    value.Add(node.InnerText);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

            return value.ToArray();
        }
        #endregion

        #region XML 노드 리스트를 가져온다.
        /// <summary>
        /// path에 해당하는 XmlNodeList 를 가져온다.
        /// </summary>
        /// <param name="xpath"></param>
        /// <returns></returns>
        public XmlNodeList GetNodeList(string xpath)
        {
            if (xmlDoc == null) return null;

            XmlNodeList list = null;

            try
            {
                list = xmlDoc.DocumentElement.SelectNodes(xpath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                list = null;
            }

            return list;
        }
        #endregion

        #region Node의 Attribute를 가져온다.
        public string GetAttribute(string xpath, string attribute)
        {
            if (xmlDoc == null) return "";

            string value = "";

            try
            {
                XmlNodeList list = xmlDoc.DocumentElement.SelectNodes(xpath);
                if (list.Count > 0)
                    value = list[0].Attributes[attribute].Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                value = string.Empty;
            }

            return value;
        }

        public string[] GetAttributes(string xpath, string attribute)
        {
            if (xmlDoc == null) return null;

            List<string> value = new List<string>();

            try
            {
                XmlNodeList list = xmlDoc.DocumentElement.SelectNodes(xpath);

                foreach (XmlNode node in list)
                {
                    value.Add(node.Attributes[attribute].Value);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

            return value.ToArray();
        }
        #endregion

        #region XML 파일로 저장
        public void SaveXml(string nodePath, object value)
        {
            try
            {
                XmlNode node = xmlDoc.SelectSingleNode(nodePath);

                if (node == null)
                {
                    string[] p = SplitString(nodePath, "/");

                    for (int i = 0; i < p.Length - 1; i++)
                    {
                        if (xmlDoc.SelectSingleNode(string.Join("/", p, 0, i + 2)) == null)
                        {
                            string key = string.Join("/", p, 0, i + 1);
                            AddXmlNode(key, p[i + 1]);
                        }
                    }

                    node = xmlDoc.SelectSingleNode(nodePath);
                }

                node.InnerText = value.ToString();
                xmlDoc.Save(docPath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
            }
        }

        private void AddXmlNode(string pNode, string xpath)
        {
            XmlNode node = xmlDoc.SelectSingleNode(pNode);
            XmlElement ele = xmlDoc.CreateElement(xpath);
            node.AppendChild(ele);
        }
        #endregion

        /// <summary>
        /// 문자열 split ex) string[] tmpstr = StringUtil.SplitString("원본문자열", "자를문자열");
        /// </summary>
        /// <param name="orgString"></param>
        /// <param name="Separator"></param>
        /// <returns></returns>
        public static string[] SplitString(string orgString, string Separator)
        {
            int offset = 0;
            int index = 0;
            int[] offsets = new int[orgString.Length + 1];

            while (index < orgString.Length)
            {
                int indexOf = orgString.IndexOf(Separator, index);
                if (indexOf != -1)
                {
                    offsets[offset++] = indexOf;
                    index = (indexOf + Separator.Length);
                }
                else
                {
                    index = orgString.Length;
                }
            }

            string[] final = new string[offset + 1];
            if (offset == 0)
            {
                final[0] = orgString;
            }
            else
            {
                offset--;
                final[0] = orgString.Substring(0, offsets[0]);
                for (int i = 0; i < offset; i++)
                {
                    final[i + 1] = orgString.Substring(offsets[i] + Separator.Length, offsets[i + 1] - offsets[i] - Separator.Length);
                }
                final[offset + 1] = orgString.Substring(offsets[offset] + Separator.Length);
            }
            return final;
        }
    }
}

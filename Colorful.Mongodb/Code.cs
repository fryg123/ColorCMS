using System;
using System.Collections.Generic;
using System.Linq;

namespace Colorful.Models
{
    /// <summary>
    /// �����ֵ�
    /// </summary>
    public class Code : BaseIntId
    {
        /// <summary>
        /// �˵����
        /// </summary>
        public long MenuId { get; set; }
        /// <summary>
        /// �ֵ����
        /// </summary>
        [BindField("���", MenuFieldType.Select)]
        public int Sort { get; set; }
        /// <summary>
        /// �ֵ��ʶ
        /// </summary>
        [BindField("�ֵ��ʶ", MenuFieldType.Select)]
        public CodeFlag Flag { get; set; }
        /// <summary>
        /// ��Id
        /// </summary>
        public int ParentId { get; set; }
        /// <summary>
        /// ����
        /// </summary>
        [BindField("����")]
        public string Name { get; set; }
        /// <summary>
        /// Ӣ������
        /// </summary>
        [BindField("Ӣ������")]
        public string NameEN { get; set; }
        /// <summary>
        /// ͼ��
        /// </summary>
        public string Icon { get; set; }
        /// <summary>
        /// ����
        /// </summary>
        public int ByOrder { get; set; }
        /// <summary>
        /// ��������
        /// </summary>
        [BindField("����")]
        public string Data { get; set; }
        /// <summary>
        /// ���
        /// </summary>
        [BindField("�����ʶ")]
        public List<int> Flags { get; set; }
        [BindField("����")]
        public string Lang { get; set; }
    }
}

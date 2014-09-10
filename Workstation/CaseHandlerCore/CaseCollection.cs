using System.Collections;
using L3.Cargo.Common;

namespace L3.Cargo.Workstation.CaseHandlerCore
{
    class CaseCollection : CollectionBase
    {
        #region Constructors

        public CaseCollection()
        {
        }

        #endregion Constructors


        #region Public Methods

        public void Add(CaseObject caseObj)
        {
            this.List.Add(caseObj);
        }

        public void Remove(CaseObject caseObj)
        {
            foreach (DataAttachment dataAttachment in caseObj.attachments)
            {
                dataAttachment.attachmentData.Dispose();
            }

            foreach (DataAttachment dataAttachment in caseObj.NewAttachments)
            {
                dataAttachment.attachmentData.Dispose();
            }

            caseObj.attachments.Clear();
            caseObj.NewAttachments.Clear();

            this.List.Remove(caseObj);
        }

        public new void RemoveAt(int index)
        {
            CaseObject caseObj = this.List[index] as CaseObject;
            if (caseObj != null)
            {
                foreach (DataAttachment dataAttachment in caseObj.attachments)
                {
                    dataAttachment.attachmentData.Dispose();
                }

                foreach (DataAttachment dataAttachment in caseObj.NewAttachments)
                {
                    dataAttachment.attachmentData.Dispose();
                }

                caseObj.attachments.Clear();
                caseObj.NewAttachments.Clear();
            }
            this.List.RemoveAt(index);
        }

        public new void Clear()
        {
            foreach (CaseObject caseObj in this.List)
            {
                foreach (DataAttachment dataAttachment in caseObj.attachments)
                {
                    dataAttachment.attachmentData.Dispose();
                }

                foreach (DataAttachment dataAttachment in caseObj.NewAttachments)
                {
                    dataAttachment.attachmentData.Dispose();
                }

                caseObj.attachments.Clear();
                caseObj.NewAttachments.Clear();
            }

            this.List.Clear();
        }

        public CaseObject Find (string caseName)
        {
            CaseObject toReturn = null;

            foreach (CaseObject cases in this.List)
            {
                if (string.Equals(cases.CaseId, caseName))
                {
                    toReturn = cases;
                    break;
                }
            }
            return toReturn;
        }

        #endregion Public Methods
    }
}

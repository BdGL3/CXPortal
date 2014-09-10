using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.Windows.Controls;
using System.Data;
using System.Windows.Data;

namespace L3.Cargo.Communications.Common
{
    public class CaseListSort: IComparer
    { 
        public delegate int TwoArgDelegate(CaseListDataSet.CaseListTableRow arg1, CaseListDataSet.CaseListTableRow arg2);
        TwoArgDelegate myComparer;

        public CaseListSort(ListSortDirection direction, DataGridColumn column)
        {
           int dir = (direction == ListSortDirection.Ascending) ? 1: -1;

            string path = BindingOperations.GetBindingExpression(column, DataGridColumn.HeaderProperty).ParentBinding.Path.Path;
            switch (path)
            {
                case "CaseId":
                    myComparer = (a, b) => { return a.CaseId.CompareTo(b.CaseId) * dir; };
                    break;

                case "AnalystComment":
                    myComparer = (a, b) => { return a.AnalystComment.CompareTo(b.AnalystComment) * dir;};
                    break;

                case "ObjectId":
                    myComparer = (a, b) => { return a.ObjectId.CompareTo(b.ObjectId) * dir;};
                    break;

                case "FlightNumber":
                    myComparer = (a, b) => { return a.FlightNumber.CompareTo(b.FlightNumber) * dir; };
                    break;

                case "Analyst":
                    myComparer = (a, b) => { return a.Analyst.CompareTo(b.Analyst) * dir; };
                    break;

                case "CaseDirectory":
                    myComparer = (a, b) => { return a.CaseDirectory.CompareTo(b.CaseDirectory) * dir; };
                    break;

                case "ReferenceImage":
                    myComparer = (a, b) => { return a.ReferenceImage.CompareTo(b.ReferenceImage) * dir; };
                    break;

                case "Result":
                    myComparer = (a, b) => { return a.Result.CompareTo(b.Result) * dir; };
                    break;

                case "UpdateTime":
                    myComparer = (a, b) => { return a.UpdateTime.CompareTo(b.UpdateTime) * dir; };
                    break;

                case "CreateTime":
                    myComparer = (a, b) => { return a.CreateTime.CompareTo(b.CreateTime) * dir; };
                    break;

                case "Archived":
                    myComparer = (a, b) => { return a.Archived.CompareTo(b.Archived) * dir; };
                    break;
                    
                case "AnalysisTime":
                    myComparer = (a, b) => { return a.AnalysisTime.CompareTo(b.AnalysisTime) * dir; };
                    break;

                default:
                    myComparer = (a, b) => { return 0; };
                    break;
            }
        }

        int IComparer.Compare(object x, object y)
        {
            DataRowView xView = (DataRowView) x;
            DataRowView yView = (DataRowView)y;

            return myComparer((CaseListDataSet.CaseListTableRow)xView.Row, (CaseListDataSet.CaseListTableRow)yView.Row);
        }                   
    }
}

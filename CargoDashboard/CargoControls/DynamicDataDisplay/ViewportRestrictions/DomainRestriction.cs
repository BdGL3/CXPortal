using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Research.DynamicDataDisplay.Common;
using Microsoft.Research.DynamicDataDisplay.ViewportRestrictions;

namespace Microsoft.Research.DynamicDataDisplay.ViewportConstraints
{
	/// <summary>
	/// Represents a constraint which returns data rectangle, intersected with specified data domain.
	/// </summary>
    public class DomainRestriction : ViewportRestrictionBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DomainConstraint"/> class.
		/// </summary>
		public DomainRestriction() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="DomainConstraint"/> class with given domain property.
		/// </summary>
		/// <param name="domain">The domain.</param>
        public DomainRestriction(DataRect domain)
		{
			this.Domain = domain;
		}

		private DataRect domain = new DataRect(-1, -1, 2, 2);
		/// <summary>
		/// Gets or sets the domain.
		/// </summary>
		/// <value>The domain.</value>
		public DataRect Domain
		{
			get { return domain; }
			set
			{
				if (domain != value)
				{
					domain = value;
					RaiseChanged();
				}
			}
		}

		/// <summary>
		/// Applies the specified old data rect.
		/// </summary>
		/// <param name="oldDataRect">The old data rect.</param>
		/// <param name="newDataRect">The new data rect.</param>
		/// <param name="viewport">The viewport.</param>
		/// <returns></returns>
        /// 
		public override Rect Apply(Rect oldDataRect, Rect newDataRect, Viewport2D viewport)
		{
			DataRect res = domain;
			if (domain.IsEmpty)
			{
				res = newDataRect;
			}
			else if (newDataRect.IntersectsWith(domain))
			{
				res = newDataRect;
                if (newDataRect.Size == oldDataRect.Size)
                {
                    if (res.XMin < domain.XMin) res.XMin = domain.XMin;
                    if (res.YMin < domain.YMin) res.YMin = domain.YMin;
                    if (res.XMax > domain.XMax) res.XMin += domain.XMax - res.XMax;
                    if (res.YMax > domain.YMax) res.YMin += domain.YMax - res.YMax;
                }
                else
                {
                    newDataRect.Intersect(domain);
                    res = newDataRect;
                }
			}

			return res;
		}
	}
}

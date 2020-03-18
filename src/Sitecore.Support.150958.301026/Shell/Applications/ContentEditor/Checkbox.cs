using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Sitecore.Support.Shell.Applications.ContentEditor
{
    public class Checkbox : Sitecore.Support.Web.UI.HtmlControls.Checkbox
    {

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Checkbox"/> class.
        /// </summary>
        public Checkbox() : base()
        {
            Class = "scContentControlCheckbox";
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public override bool ReadOnly
        {
            get
            {
                return this.GetViewStateBool("ReadOnly");
            }
            set
            {
                this.SetViewStateBool("ReadOnly", value);

                if (value)
                {
                    this.Attributes["readonly"] = "readonly";
                    this.Disabled = true;
                }
                else
                {
                    this.Attributes.Remove("readonly");
                }
            }
        }
        #endregion

        #region Protected methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender"></see> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"></see> object that contains the event data.</param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // ViewState work-around
            ServerProperties["Value"] = ServerProperties["Value"];
            ServerProperties["Checked"] = ServerProperties["Checked"];
        }

        /// <summary>
        /// Sets the modified flag.
        /// </summary>
        protected override void SetModified()
        {
            base.SetModified();

            if (TrackModified)
            {
                Sitecore.Context.ClientPage.Modified = true;
            }
        }

        #endregion
    }
}
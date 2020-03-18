using System;
using System.Web.UI;
using Sitecore.Diagnostics;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;

namespace Sitecore.Support.Web.UI.HtmlControls
{
    public class Checkbox : Input
    {

        #region Public events

        /// <summary></summary>
        public event EventHandler OnClick;

        #endregion

        #region Fields

        bool m_assigned;
        bool m_checked;
        string m_loadedValue;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Checkbox"/> class.
        /// </summary>
        public Checkbox()
        {
            m_loadedValue = null;
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Checkbox"/> is checked.
        /// </summary>
        /// <value><c>true</c> if checked; otherwise, <c>false</c>.</value>
        public bool Checked
        {
            get
            {
                if (m_assigned)
                {
                    return m_checked;
                }

                return GetViewStateBool("Checked");
            }
            set
            {
                if (value != Checked)
                {
                    m_checked = value;
                    m_assigned = true;

                    SetViewStateBool("Checked", value);

                    SheerResponse.SetAttribute(ID, "checked", value ? "true" : "false");

                    if (value)
                    {
                        Attributes["Checked"] = "Checked";
                    }
                    else
                    {
                        Attributes.Remove("Checked");
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the click event.
        /// </summary>
        /// <value>The click event.</value>
        public string Click
        {
            get
            {
                return GetViewStateString("Click");
            }
            set
            {
                SetViewStateString("Click", value);
            }
        }

        /// <summary>
        /// Gets or sets the header.
        /// </summary>
        /// <value>The header.</value>
        public virtual string Header
        {
            get
            {
                return GetViewStateString("Header");
            }
            set
            {
                Error.AssertString(value, "Header", true);
                SetViewStateString("Header", value);
            }
        }

        /// <summary>
        /// Gets or sets the header CSS style.
        /// </summary>
        /// <value>The header CSS style.</value>
        public virtual string HeaderStyle
        {
            get
            {
                return GetViewStateString("HeaderStyle");
            }
            set
            {
                Error.AssertString(value, "HeaderStyle", true);
                SetViewStateString("HeaderStyle", value);
            }
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public override string Value
        {
            get
            {
                return Checked ? "1" : "";
            }
            set
            {
                Checked = (value == "1");
            }
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Executes the Change event.
        /// </summary>
        /// <param name="message">The message.</param>
        protected override void DoChange(Message message)
        {
            Error.AssertObject(message, "message");

            Checked = (Sitecore.Context.ClientPage.ClientRequest.Form[ID] != null);

            base.DoChange(message);
        }

        /// <summary>
        /// Executes the click event.
        /// </summary>
        /// <param name="message">The message.</param>
        protected virtual void DoClick(Message message)
        {
            Error.AssertObject(message, "message");

            if (OnClick != null)
            {
                OnClick(this, EventArgs.Empty);
            }

            SheerResponse.SetReturnValue(true);
        }

        /// <summary>
        /// Renders the control.
        /// </summary>
        /// <param name="output">The output.</param>
        protected override void DoRender(HtmlTextWriter output)
        {
            Attributes["value"] = "1";

            string click = string.Empty;

            if (Click.Length > 0)
            {
                if (Message.IsMessage(click))
                {
                    click = Sitecore.Context.ClientPage.GetClientEvent(click);
                }
                else
                {
                    click = Sitecore.Context.ClientPage.GetClientEvent(ID + "." + click);
                }

                click = " onclick=\"" + click + "\"";
            }

            output.Write("<input" + ControlAttributes + " type=\"checkbox\"" + click + "/>");

            // render header
            if (Header.Length > 0 && Visible)
            {
                bool hasLabel = !string.IsNullOrEmpty(ID);

                if (hasLabel)
                {
                    output.Write("<label id=\"" + ID + "_label\" for=\"" + ID + "\">");
                }

                output.Write(UIUtil.FormatHeader(Header, HeaderStyle));

                RenderChildren(output);

                if (hasLabel)
                {
                    output.Write("</label>");
                }
            }
        }

        /// <summary>
        /// Loads the post data.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        protected override bool LoadPostData(string value)
        {
            m_loadedValue = value;

            return false;
        }

        /// <summary>
        /// When implemented by a class, processes postback data for an ASP.NET server control.
        /// </summary>
        /// <param name="postDataKey">The key identifier for the control.</param>
        /// <param name="postCollection">The collection of all incoming name values.</param>
        /// <returns>
        /// true if the server control's state changes as a result of the postback; otherwise, false.
        /// </returns>
        public override bool LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
        {
            return LoadPostData(postCollection[ID]);
        }

        /// <summary>
        /// Raises the <see cref="System.Web.UI.Control.Load"></see> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"></see> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (Sitecore.Context.ClientPage.IsEvent && !Sitecore.Context.ClientPage.IsResult)
            {
                bool isChecked = m_loadedValue != null;

                if (isChecked != Checked)
                {
                    SetModified();

                    Checked = isChecked;
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (Page != null && IsEnabled)
            {
                Page.RegisterRequiresPostBack(this);
            }
        }

        #endregion
    }
}
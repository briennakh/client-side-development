﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestUtility;
using System.Diagnostics;

namespace Project3
{
    public partial class Form1 : Form
    {

        // global objects (to make json data accessible after retrieving)
        About about;
        Degrees degrees;
        Minors minors;
        People people;
        Research research;
        Employment employment;
        News news;
        Resources resources;
        Footer footer;
        
        // get our restful resources...
        REST rj = new REST("http://ist.rit.edu/api");
        // another one we won't use
        REST rx = new REST("http://google.com/api");

        // stopwatch for testing...
        Stopwatch sw = new Stopwatch();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Customize body tab control
            body.Appearance = TabAppearance.FlatButtons;
            body.ItemSize = new Size(0, 1);
            body.SizeMode = TabSizeMode.Fixed;

            // go get the /resources/ info...
            string jsonResources = rj.getRestJSON("/resources/");
            resources = JToken.Parse(jsonResources).ToObject<Resources>(); // (parses returned json and casts to object) Resources.cs with json2csharp

            ll_pdf.Tag = resources.coopEnrollment.RITJobZoneGuidelink;

            ll_resources_appForm.Text = "Get the application form";
            ll_resources_appForm.Tag = resources.studentAmbassadors.applicationFormLink;
            ll_resources_appForm.LinkClicked += linkLabel1_LinkClicked;


            /*
             * Let's assume that we are going to get a list of acceptable tabs to show the user from a call
             */

            // retrieved from the server
            string[] names = { "Degrees", "People", "ListView", "DataGridView", "Employment" };

            // look through all tabpages
            foreach (TabPage tab in TabControl1.TabPages)
            {
                // is this tab in the names?
                if (!names.Contains(tab.Text))
                {
                    // get which one
                    int t = TabControl1.TabPages.IndexOf(tab);
                    TabControl1.TabPages.RemoveAt(t);
                }
            }

            // create a new tabPage
            TabPage myNewTab = new TabPage("News");
            TabControl1.TabPages.Add(myNewTab);

            // create something for this tabPage
            TextBox tb = new TextBox();
            tb.BackColor = SystemColors.HotTrack;
            tb.Location = new Point(30, 30);
            tb.Size = new Size(180, 300);
            tb.TabIndex = 1;
            // put on the page
            myNewTab.Controls.Add(tb);
        }

        #region GetRest
        public string getRestData(string uri)
        {
            string baseUri = "http://ist.rit.edu/api";
            // connect to the api
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(baseUri + uri);
            try
            {
                WebResponse resp = req.GetResponse();
                using (Stream respStream = resp.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(respStream, Encoding.UTF8);
                    return reader.ReadToEnd();
                }
            } catch (WebException ex)
            {
                WebResponse err = ex.Response;
                using (Stream respStream = err.GetResponseStream())
                {
                    StreamReader r = new StreamReader(respStream, Encoding.UTF8);
                    string errorText = r.ReadToEnd();
                    // log it
                }
                throw;
            }
        
        }
        #endregion

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // which link am I?
            LinkLabel me = sender as LinkLabel;
            // me
            me.LinkVisited = true;
            // navigate to the url
            System.Diagnostics.Process.Start(me.Tag.ToString());
        }

        private void btn_people_Click(object sender, EventArgs e)
        {
            // get the people
            string jsonPeople = rj.getRestJSON("/people/");
            // cast people object
            people = JToken.Parse(jsonPeople).ToObject<People>();

            // play with the data...
            foreach(Faculty thisFac in people.faculty)
            {
                Console.WriteLine(thisFac.name);
                pb_fac.Load(thisFac.imagePath);
            }

            // the ability to get a single instance of Faculty from the username
            getSingleInstance("dsbics");

        }

        private void getSingleInstance(string id)
        {
            /*
             * We are going to need a way to find a specific instance of a list of objects
             * to send to the other form for display 
             */

            // horrible, ugly, don't do it this way - way... will continue searching even after finding
            /*
            foreach(Faculty thisFac in people.faculty)
            {
                if (thisFac.username == id)
                {
                    MessageBox.Show(thisFac.name);
                }
            }*/

            // better way
            Faculty result = people.faculty.Find(x => x.username == id);
            MessageBox.Show(result.name);

            // OR
            List<Faculty> facList = people.faculty.FindAll(x => x.title == "Associate Professor");
            Console.WriteLine(facList[2].name);

        }

        private void DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // MessageBox.Show("i");
            int col = e.ColumnIndex; // won't use
            int row = e.RowIndex;

            string id = DataGridView1[0, row].Value.ToString();
            MessageBox.Show(id);
        }

        private void btn_ListView_Click(object sender, EventArgs e)
        {
            // how long does this take? (remember to remove stopwatch when deploy app, remove System.Diagnostics)
            sw.Reset();
            sw.Start();

            // dynamically create 
            listView1.View = View.Details; // we want text
            listView1.GridLines = true;
            listView1.FullRowSelect = true; // click, highlight full row
            listView1.Width = 710;

            // create the column headers...
            listView1.Columns.Add("Employers", 200); // 2nd arg is how big it shld be
            listView1.Columns.Add("Degrees", 200);
            listView1.Columns.Add("City", 200);
            listView1.Columns.Add("Term", 100);

            // and populate ListView
            ListViewItem item;

            for (int i = 0; i < employment.coopTable.coopInformation.Count; i++)
            {
                item = new ListViewItem(new String[] {
                    employment.coopTable.coopInformation[i].employer,
                    employment.coopTable.coopInformation[i].degree,
                    employment.coopTable.coopInformation[i].city,
                    employment.coopTable.coopInformation[i].term
                });

                listView1.Items.Add(item);
            }

            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds.ToString());
        }

        private void tabPage5_Enter(object sender, EventArgs e)
        {
            // do i have the data?
            if (employment == null)
            {
                MessageBox.Show("load");
                // go get the /employment info
                string jsonEmp = rj.getRestJSON("/employment/");
                employment = JToken.Parse(jsonEmp).ToObject<Employment>();

            }

            // have I been built?
            if (DataGridView1.Rows.Count < 2)
            {
                // start stopwatch
                sw.Reset();
                sw.Start();

                // populate the dataGridView...
                for (int i = 0; i < employment.coopTable.coopInformation.Count; i++)
                {
                    DataGridView1.Rows.Add();
                    DataGridView1.Rows[i].Cells[0].Value = employment.coopTable.coopInformation[i].employer;
                    DataGridView1.Rows[i].Cells[1].Value = employment.coopTable.coopInformation[i].degree;
                    DataGridView1.Rows[i].Cells[2].Value = employment.coopTable.coopInformation[i].city;
                    DataGridView1.Rows[i].Cells[3].Value = employment.coopTable.coopInformation[i].term;
                }

                sw.Stop();
                Console.WriteLine(sw.ElapsedMilliseconds.ToString());
            }
        }

        // DELETE
        private void tab_degrees_Enter(object sender, EventArgs e)
        {
        }

        private void about_btn_MouseHover(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.Hand;
        }

        // When "ABOUT" is clicked
        private void about_btn_Click(object sender, EventArgs e) {
            // Change body view to About section
            body.SelectedTab = about_tab;

            // Ensure we have the data, fetch if we don't
            if (about == null) {
                Console.WriteLine("Loading about...");
                string jsonAbout = rj.getRestJSON("/about/");
                about = JToken.Parse(jsonAbout).ToObject<About>();

                // Dynamically load About section page
                about_title.Text = about.title;
                about_desc.Text = about.description;
                about_quote.Text = about.quote;
                about_quoteAuth.Text = about.quoteAuthor;
            }
        }

        // Change body view to Degrees section when "DEGREES" is clicked
        private void degrees_btn_Click(object sender, EventArgs e) {
            // Change body view to Degrees section
            body.SelectedTab = degrees_tab;

            // Ensure we have the data, fetch if we don't
            if (degrees == null) {
                Console.WriteLine("Loading degrees...");
                string jsonDeg = rj.getRestJSON("/degrees/");
                degrees = JToken.Parse(jsonDeg).ToObject<Degrees>();

                // Dynamically load undergraduate degrees
                int row = 0;
                int column = 0;
                for (int i = 0; i < degrees.undergraduate.Count; i++) {
                    // Create and populate panel for each degree
                    TableLayoutPanel degreePanel = new TableLayoutPanel();
                    degreePanel.ColumnCount = 1;
                    degreePanel.RowCount = 2;
                    degreePanel.AutoSize = true;
                    degreePanel.Dock = DockStyle.Fill;
                    degreePanel.BorderStyle = BorderStyle.FixedSingle;
                    foreach (RowStyle style in degreePanel.RowStyles) {
                        style.SizeType = SizeType.AutoSize;
                    }

                    // Degree title
                    Label degTitle = new Label();
                    degTitle.Text = degrees.undergraduate[i].title;
                    degTitle.Dock = DockStyle.Fill;
                    degTitle.AutoSize = false;
                    degTitle.MaximumSize = new Size(100, 0);
                    degTitle.AutoSize = true;

                    // Degree description
                    TextBox degDesc = new TextBox();
                    degDesc.ReadOnly = true;
                    degDesc.Multiline = true;
                    degDesc.Dock = DockStyle.Fill;
                    degDesc.Text = degrees.undergraduate[i].description;
                    SizeF size = degDesc.CreateGraphics()
                                .MeasureString(degDesc.Text,
                                                degDesc.Font,
                                                degDesc.Width,
                                                new StringFormat(0));
                    degDesc.Height = (int)size.Height;

                    // Add components to degree panel, then to main panel
                    degreePanel.Controls.Add(degTitle, 0, 0);
                    degreePanel.Controls.Add(degDesc, 0, 1);
                    ug_degrees.Controls.Add(degreePanel, column, row);

                    // Resize rows
                    foreach (RowStyle style in ug_degrees.RowStyles) {
                        style.SizeType = SizeType.AutoSize;
                    }

                    // Set onclick event handler to show degree details in popup
                    string s = " hi ";
                    degreePanel.Click += (sender2, e2) => degree_click(sender2, e2, s);

                    // Jump to next row if current row is full
                    if ((i + 1) % 3 == 0) {
                        row++;
                        column = 0;
                    } else {
                        column++;
                    }
                }
                
                // Dynamically load graduate degrees and certificates
                row = 0;
                column = 0;
                for (int i = 0; i < degrees.graduate.Count; i++) {
                    // If certificate,
                    if (degrees.graduate[i].degreeName == "graduate advanced certificates") {
                        int certRow = 0;
                        int certColumn = 0;
                        for (int j = 0; j < degrees.graduate[i].availableCertificates.Count; j++) {
                            // Create and populate panel for each certificate
                            TableLayoutPanel certPanel = new TableLayoutPanel();
                            certPanel.ColumnCount = 1;
                            certPanel.RowCount = 1;
                            certPanel.AutoSize = true;
                            certPanel.Dock = DockStyle.Fill;
                            certPanel.BorderStyle = BorderStyle.FixedSingle;
                            foreach (RowStyle style in certPanel.RowStyles) {
                                style.SizeType = SizeType.AutoSize;
                            }

                            // Certificate title
                            LinkLabel certTitle = new LinkLabel();
                            certTitle.Text = degrees.graduate[i].availableCertificates[j];
                            certTitle.Dock = DockStyle.Fill;
                            certTitle.AutoSize = false;
                            certTitle.MaximumSize = new Size(100, 0);
                            certTitle.AutoSize = true;
                            if (certTitle.Text == "Web Development Advanced certificate") {
                                certTitle.Tag = "http://www.rit.edu/programs/web-development-adv-cert";
                            } else if (certTitle.Text == "Networking,Planning and Design Advanced Cerificate") {
                                certTitle.Tag = "http://www.rit.edu/programs/networking-planning-and-design-adv-cert";
                            }

                            // Add components to certificate panel, then to main panel
                            certPanel.Controls.Add(certTitle, 0, 0);
                            grad_certs.Controls.Add(certPanel, certColumn, certRow);

                            // Resize rows
                            foreach (RowStyle style in grad_certs.RowStyles) {
                                style.SizeType = SizeType.AutoSize;
                            }

                            // Jump to next row if current row is full
                            if ((j + 1) % 3 == 0) {
                                certRow++;
                                certColumn = 0;
                            } else {
                                certColumn++;
                            }
                        }

                        // End current iteration of i loop here
                        continue;
                    }

                    // Create and populate panel for each degree
                    TableLayoutPanel degreePanel = new TableLayoutPanel();
                    degreePanel.ColumnCount = 1;
                    degreePanel.RowCount = 2;
                    degreePanel.AutoSize = true;
                    degreePanel.Dock = DockStyle.Fill;
                    degreePanel.BorderStyle = BorderStyle.FixedSingle;
                    foreach (RowStyle style in degreePanel.RowStyles) {
                        style.SizeType = SizeType.AutoSize;
                    }

                    // Degree title
                    Label degTitle = new Label();
                    degTitle.Text = degrees.graduate[i].title;
                    degTitle.Dock = DockStyle.Fill;
                    degTitle.AutoSize = false;
                    degTitle.MaximumSize = new Size(100, 0);
                    degTitle.AutoSize = true;

                    // Degree description
                    TextBox degDesc = new TextBox();
                    degDesc.ReadOnly = true;
                    degDesc.Multiline = true;
                    degDesc.Dock = DockStyle.Fill;
                    degDesc.Text = degrees.graduate[i].description;
                    SizeF size = degDesc.CreateGraphics()
                                .MeasureString(degDesc.Text,
                                                degDesc.Font,
                                                degDesc.Width,
                                                new StringFormat(0));
                    degDesc.Height = (int)size.Height;

                    // Add components to degree panel, then to main panel
                    degreePanel.Controls.Add(degTitle, 0, 0);
                    degreePanel.Controls.Add(degDesc, 0, 1);
                    grad_degrees.Controls.Add(degreePanel, column, row);

                    // Resize rows
                    foreach (RowStyle style in grad_degrees.RowStyles)
                    {
                        style.SizeType = SizeType.AutoSize;
                    }

                    // Set onclick event handler to show degree details in popup
                    string s = " hi ";
                    degreePanel.Click += (sender2, e2) => degree_click(sender2, e2, s);

                    // Jump to next row if current row is full
                    if ((i + 1) % 3 == 0)
                    {
                        row++;
                        column = 0;
                    } else {
                        column++;
                    }
                }


            }
        }

        private void degree_click(object sender, EventArgs e, string deg)
        {
            Overlay overlay = new Overlay(deg);
            overlay.Show();
        }

        // Change body view to People section when "PEOPLE" is clicked
        private void people_btn_Click(object sender, EventArgs e)
        {
            body.SelectedTab = people_tab;
        }
        // Change body view to Research section when "RESEARCH" is clicked
        private void research_btn_Click(object sender, EventArgs e)
        {
            body.SelectedTab = research_tab;
        }
        // Change body view to Employment section when "EMPLOYMENT" is clicked
        private void emp_btn_Click(object sender, EventArgs e)
        {
            body.SelectedTab = emp_tab;
        }
        // Change body view to Resources section when "RESOURCES" is clicked
        private void resources_btn_Click(object sender, EventArgs e)
        {
            body.SelectedTab = resources_tab;
        }

        // Handle minors data when entering "Minors" tab
        private void tabControl3_Enter(object sender, EventArgs e) {
            // Ensure we have the data, fetch if we don't
            if (minors == null) {
                Console.WriteLine("Loading minors...");
                string jsonMinors = rj.getRestJSON("/minors/");
                minors = JToken.Parse(jsonMinors).ToObject<Minors>();

                // Dynamically load minors
                int row = 0;
                int column = 0;
                for (int i = 0; i < minors.UgMinors.Count; i++) {
                    // Create and populate panel for each minor
                    TableLayoutPanel minorPanel = new TableLayoutPanel();
                    minorPanel.ColumnCount = 1;
                    minorPanel.RowCount = 1;
                    minorPanel.AutoSize = true;
                    minorPanel.Dock = DockStyle.Fill;
                    minorPanel.BorderStyle = BorderStyle.FixedSingle;
                    foreach (RowStyle style in minorPanel.RowStyles) {
                        style.SizeType = SizeType.AutoSize;
                    }

                    // Minor title
                    Label minorTitle = new Label();
                    minorTitle.Text = minors.UgMinors[i].title;
                    minorTitle.Dock = DockStyle.Fill;
                    minorTitle.AutoSize = false;
                    minorTitle.MaximumSize = new Size(100, 0);
                    minorTitle.AutoSize = true;

                    // Add components to degree panel, then to main panel
                    minorPanel.Controls.Add(minorTitle, 0, 0);
                    ug_minors.Controls.Add(minorPanel, column, row);

                    // Resize rows
                    foreach (RowStyle style in ug_minors.RowStyles) {
                        style.SizeType = SizeType.AutoSize;
                    }

                    // Set onclick event handler to show degree details in popup
                    string s = " hi ";
                    minorPanel.Click += (sender2, e2) => degree_click(sender2, e2, s);

                    // Jump to next row if current row is full
                    if ((i + 1) % 3 == 0) {
                        row++;
                        column = 0;
                    } else {
                        column++;
                    }
                }
            }
        }
    }
}

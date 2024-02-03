using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace WpfAppProjekty
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private class Project
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Abbreviation { get; set; }
            public string Customer { get; set; }
            public Project(string id, string name, string abbreviation, string customer)
            {
                Id = id;
                Name = name;
                Abbreviation = abbreviation;
                Customer = customer;
            }
        }
        
        private List<Project> m_projects = new List<Project>(); //kolekcia projektov

        private localhost.WebServiceProjekty m_mojSvc; //server

        public MainWindow()
        {
            InitializeComponent();
            Closing += MainWindow_Closing;
            Closed += MainWindow_Closed;

            try
            {
                m_mojSvc = new localhost.WebServiceProjekty(); //napojenie na server

                //vypytanie hesla od uzivatela a overenie
                PasswordDlg dlg = new PasswordDlg();
                if (dlg.ShowDialog() != true) throw new Exception("Password is required."); //bool?
                if (!m_mojSvc.Hello(dlg.Password)) throw new Exception("Failed to establish a connection to the server."); //patri sa pozdravit server

                listView.ItemsSource = m_projects; //previazanie ListView s kolekciou projektov
                buttonRefresh_Click(null, null); //naplnenie kolekcie projektov (a zaroven aj ListView)
            }
            catch (Exception ex)
            {
                m_mojSvc = null;
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                Application.Current.Shutdown(); //ukoncenie aplikacie!
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) //zatvaranie hlavneho okna mozno zamedzit
        {
            if (m_mojSvc != null)
            {
                MessageBoxResult result = MessageBox.Show("Do you really want to close the app?", "Exit", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result != MessageBoxResult.Yes)
                {
                    e.Cancel = true; //uzivatel nechce alebo zmatkuje
                }
            }
        }

        private void MainWindow_Closed(object sender, EventArgs e) //definitivne zatvaranie hlavneho okna aplikacie
        {
            if (m_mojSvc != null)
            {
                m_mojSvc.Bye(); //patri sa rozlucit so serverom
                m_mojSvc = null;
            }
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e) //zmena vybraneho riadku v ListView
        {
            if (listView.SelectedItem == null) //nic nie je aktualne vybrate
            {
                textBoxId.Text = "";
                textBoxName.Text = "";
                textBoxAbbreviation.Text = "";
                textBoxCustomer.Text = "";
            }
            else
            {
                Project prj = (Project)listView.SelectedItem; //aktualne vybraty objekt
                textBoxId.Text = prj.Id;
                textBoxName.Text = prj.Name;
                textBoxAbbreviation.Text = prj.Abbreviation;
                textBoxCustomer.Text = prj.Customer;
            }
        }

        private void buttonRefresh_Click(object sender, RoutedEventArgs e) //button "Refresh"
        {
            listView.ItemsSource = null;

            try
            {
                m_projects.Clear();

                string[] ids = m_mojSvc.GetProjectsIds();
                foreach (var id in ids)
                {
                    string name, abbreviation, customer;
                    if (!m_mojSvc.GetProject(id, out name, out abbreviation, out customer)) throw new Exception(string.Format("Server is unable to get project Id={0}!", id));
                    m_projects.Add(new Project(id, name, abbreviation, customer)); //novy objekt sa prida do kolekcie
                }

                listView.ItemsSource = m_projects;
            }
            catch (Exception ex)
            {
                m_projects.Clear(); //toto by sa nemalo stat!
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            listView.UpdateLayout();
            listView_SelectionChanged(null, null);
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e) //button "Add"
        {
            try
            {
                string id = textBoxId.Text;
                string name = textBoxName.Text;
                string abbreviation = textBoxAbbreviation.Text;
                string customer = textBoxCustomer.Text;

                if (!m_mojSvc.AddProject(id, name, abbreviation, customer)) throw new Exception(string.Format("Server is unable to add new project Id={0}!", id));

                m_projects.Add(new Project(id, name, abbreviation, customer)); //novy objekt sa prida aj do kolekcie (pre zobrazenie v ListView)
                listView.Items.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void buttonUpdate_Click(object sender, RoutedEventArgs e) //button "Update"
        {
            try
            {
                if (listView.SelectedItem == null) throw new Exception("Nothing selected"); //nic nie je vybrate
                Project prj = (Project)listView.SelectedItem; //aktualne vybraty objekt
                if (textBoxId.Text != prj.Id) throw new Exception("Inconsistency"); //nekonzistentnost!

                string id = textBoxId.Text;
                string name = textBoxName.Text;
                string abbreviation = textBoxAbbreviation.Text;
                string customer = textBoxCustomer.Text;

                if (!m_mojSvc.UpdateProject(id, name, abbreviation, customer)) throw new Exception(string.Format("Server is unable to update project Id={0}!", id));

                //upravia sa hodnoty objektu
                prj.Id = id;
                prj.Name = name;
                prj.Abbreviation = abbreviation;
                prj.Customer = customer;

                listView.Items.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void buttonRemove_Click(object sender, RoutedEventArgs e) //button "Remove"
        {
            try
            {
                if (listView.SelectedItem == null) throw new Exception("Nothing selected"); //nic nie je vybrate
                Project prj = (Project)listView.SelectedItem; //aktualne vybraty objekt
                if (textBoxId.Text != prj.Id) throw new Exception("Inconsistency"); //nekonzistentnost!

                string id = textBoxId.Text;

                if (!m_mojSvc.RemoveProject(id)) throw new Exception(string.Format("Server is unable to remove project Id={0}!", id));

                m_projects.Remove(prj);  //objekt sa odstrani z kolekcie
                listView.Items.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
        private void buttonTest_Click(object sender, RoutedEventArgs e) //button "Test"
        {
            //string str = m_mojSvc.HelloWorld();
            //MessageBox.Show(str);

            //int idx = m_mojSvc.FindSubstringIndex("Kde bolo, tam bolo", "bolo");
            //MessageBox.Show(idx.ToString());

            //int idx = -100;
            //bool ok = m_mojSvc.FindSubstringIndex2("Kde bolo, tam bolo", "bolo", out idx);
            //MessageBox.Show(string.Format("idx={0} => {1}", idx, ok));

            int count = m_mojSvc.GetProjectsCount();
            string[] ids = m_mojSvc.GetProjectsIds();
            MessageBox.Show(string.Format("Počet projektov: {0}\nIds: {1}", count, string.Join(", ", ids)));
        }
    }
}

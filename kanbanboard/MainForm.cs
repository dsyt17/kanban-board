﻿using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using ContentAlignment = System.Drawing.ContentAlignment;

namespace kanbanboard
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            // Событие при изменении размера таблицы
            TableLayoutPanel.Resize += (s, a) => ResizeTable();

            Size = new Size(Size.Width + 1, Size.Height);

            // Установка двойной буферизации для устранения мерцания
            SetDoubleBuffered(TableLayoutPanel);

            // Начальные данные
            Table();

            Load += (s, a) =>
            {
                TasksButton.PerformClick();
                UsernameLabel.Text = Login.Username;

                // Событие по клику на каждый тикет. Открывает панель для выполнения изменений выбранного тикета
                TableLayoutPanel.Controls.OfType<TicketPanel>().ToList().ForEach(
                    x => x.Click += (sender, args) =>
                    {
                        // Показ панели. Возврат к тикетам. Масштабируемость
                        TicketsChangePanel.BringToFront();
                        ChangingPanel.ToCenter(TicketsChangePanel);

                        TicketsChangePanel.Click += (o, eventArgs) =>
                        {
                            TableLayoutPanel.BringToFront();
                            ChangingPanel.Controls.OfType<TextBox>().ToList().ForEach(y => y.Clear());
                        };

                        TicketsChangePanel.Resize += (o, eventArgs) => ChangingPanel.ToCenter(TicketsChangePanel);

                        // Показываем значения лейблов тикета
                        ChangingTitleTextBox.Text = GetTextFromTicket(x.Name, "Title");
                        ChangingTicketTextBox.Text = GetTextFromTicket(x.Name, "Ticket");
                        ChangingPeopleTextBox.Text = GetTextFromTicket(x.Name, "People");

                        // Изменяем при нажатии на кнопку сохранения
                        SaveChangesButton.Click += (o, eventArgs) =>
                        {
                            ChangeTextInTicket(x.Name, "Title", ChangingTitleTextBox.Text);
                            ChangeTextInTicket(x.Name, "Ticket", ChangingTicketTextBox.Text);
                            ChangeTextInTicket(x.Name, "People", ChangingPeopleTextBox.Text);
                            MessageBox.Show("Успешно сохранено", "Изменения", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        };
                    });

                // перемещение тикетов влево, вправо
                TableLayoutPanel.Controls.OfType<TicketPanel>().ToList().ForEach(x =>
                {
                    x.LeftButton.Click += (sender, b) =>
                    {
                        var column = TableLayoutPanel.GetPositionFromControl(x).Column - 1;
                        if (column < 0) return;

                        for (var i = 1; i < TableLayoutPanel.RowCount; i++)
                        {
                            if (TableLayoutPanel.GetControlFromPosition(column, i) != null) continue;
                            AddControlToPanel(x, column, i);
                            ResizeTable();
                            return;
                        }
                    };

                    x.RightButton.Click += (sender, w) =>
                    {
                        var column = TableLayoutPanel.GetPositionFromControl(x).Column + 1;
                        if (column > TableLayoutPanel.ColumnCount) return;


                        for (var i = 1; i < TableLayoutPanel.RowCount; i++)
                        {
                            if (TableLayoutPanel.GetControlFromPosition(column, i) != null) continue;
                            AddControlToPanel(x, column, i);
                            ResizeTable();
                            return;
                        }

                        // var controlPositionRow = (from Control control in TableLayoutPanel.Controls where TableLayoutPanel.GetPositionFromControl(control).Column == column select TableLayoutPanel.GetRow(control)).ToList();
                        // AddControlToPanel(x, column, controlPositionRow.Max() + 1);
                    };
                });
                
                // чтоб не было скролл полосок
                Size = new Size(Width + 3, Height + 3);
            };
        }

        private void RemoveEmptyRows()
        {
            for (int row = 0; row < TableLayoutPanel.RowCount; row++)
            {
                var count = 0;
                for (int column = 0; column < TableLayoutPanel.ColumnCount; column++)
                {
                    if (TableLayoutPanel.GetControlFromPosition(column, row) is null)
                        count++;
                }

                if (count == TableLayoutPanel.ColumnCount)
                {
                    TableLayoutPanel.RowCount--;
                }
            }
        }

        // Таблица с тикетами
        private void Table()
        {
            TableLayoutPanel.RowStyles.Clear();
            TableLayoutPanel.ColumnStyles.Clear();
            TableLayoutPanel.Controls.Clear();

            AddControlToPanel(new TicketPanel(), 0, 1);
            AddControlToPanel(new TicketPanel(), 1, 1);
            AddControlToPanel(new TicketPanel(), 2, 1);
            AddControlToPanel(new TicketPanel(), 3, 1);
            AddControlToPanel(new TicketPanel(), 0, 2);
            AddControlToPanel(new TicketPanel(), 2, 2);

            // Тикеты
            ChangeTextInTicket("ticket21", "People", "Работяги");
            ChangeTextInTicket("ticket21", "Title", "Тайтл");
            ChangeTextInTicket("ticket21", "Ticket", "Текстовый текст");
            ChangeTextInTicket("ticket21", "Title", "Текстовый текст номер два точка ноль точка");

            // Для заголовков
            // TableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Заголовки
            AddTitle("Что-то начать делать", 0);
            AddTitle("Что-то делают", 1);
            AddTitle("Что-то сделано", 2);
            AddTitle("Что-то нужно сдать", 3);
            
            BasicContentPanel.Controls.Add(TableLayoutPanel);
        }

        // Добавление заголовков
        private void AddTitle(string text, int column)
        {
            if (TableLayoutPanel.ColumnCount <= column) TableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            AddControlToPanel(new Label()
            {
                Text = text,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Tahoma", 9.75F, FontStyle.Regular),
                ForeColor = Color.FromArgb(160, 160, 160),
                Margin = new Padding(5),
                AutoSize = true
            }, column, 0);
        }

        private string GetTextFromTicket(string kanbanTicketPanelColumnRow, string whichLabel)
        {
            // kanbanTicketPanelColumnRow — ticket{column}{row}
            // whichLabel — Title/Ticket/People
            // inputText — любой текст

            try
            {
                return TableLayoutPanel.Controls.OfType<TicketPanel>().First(x => x.Name == kanbanTicketPanelColumnRow)
                    .Controls.OfType<Label>().First(x => x.Name == whichLabel)
                    .Text;
            }
            catch
            {
                return "";
            }
        }

        // Изменение текста в тикетах
        private void ChangeTextInTicket(string kanbanTicketPanelColumnRow, string whichLabel, string inputText)
        {
            // kanbanTicketPanelColumnRow — ticket{column}{row}
            // whichLabel — Title/Ticket/People
            // inputText — любой текст

            try
            {
                TableLayoutPanel.Controls.OfType<TicketPanel>().First(x => x.Name == kanbanTicketPanelColumnRow)
                    .Controls.OfType<Label>().First(x => x.Name == whichLabel)
                    .Text = inputText;
            }
            catch { }
        }

        // Добавить контрол (в основном тикет) в таблицу
        private void AddControlToPanel(Control control, int column, int row)
        {
            // Инициализация имени панели тикета
            control.Name = $"ticket{column}{row}";

            // Нужно ли добавлять доп. строки и/или колонки
            if (TableLayoutPanel.RowStyles.Count <= row)
                TableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent));
            if (TableLayoutPanel.ColumnStyles.Count <= column)
                TableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent));
            TableLayoutPanel.Controls.Add(control, column, row);
        }

        // *Для дебага. Получить позицию при клике
        private void TableLayoutPanel_MouseClick(object sender, MouseEventArgs e)
        {
            var row = 0;
            var verticalOffset = 0;
            foreach (var h in TableLayoutPanel.GetRowHeights())
            {
                var column = 0;
                var horizontalOffset = 0;
                foreach (var w in TableLayoutPanel.GetColumnWidths())
                {
                    var rectangle = new Rectangle(horizontalOffset, verticalOffset, w, h);
                    if (rectangle.Contains(e.Location))
                    {
                        if (row == 0)
                        {

                        }
                        MessageBox.Show($"row {row}, column {column} was clicked");
                        return;
                    }

                    horizontalOffset += w;
                    column++;
                }
                verticalOffset += h;
                row++;
            }
        }

        // Устранение мерцания при изменении размеров таблицы
        public static void SetDoubleBuffered(Control c)
        {
            if (SystemInformation.TerminalServerSession)
                return;
            var aProp = typeof(Control).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            aProp?.SetValue(c, true, null);
        }

        // Клик на профиль. Открытие панели с данными текущего профиля
        private void UserControlsPanel_Click(object sender, EventArgs e)
        {
            LabelHead.Text = "Профиль";

            UserPanel.BringToFront();
        }


        // Обработчик задач
        private void TasksButton_Click(object sender, EventArgs e)
        {
            LabelHead.Text = "Задачи";

            TableLayoutPanel.BringToFront();

            StripPanel.Location = TasksButton.Location;
        }


        // Обработчик мессенджера
        private void MessengerButton_Click(object sender, EventArgs e)
        {
            LabelHead.Text = "Мессенджер";

            DialogPanel.BringToFront();

            StripPanel.Location = MessengerButton.Location;
        }

        // Обработчик календаря
        private void CalendarButton_Click(object sender, EventArgs e)
        {
            LabelHead.Text = "Календарь";

            CalendarPanel.BringToFront();

            StripPanel.Location = CalendarButton.Location;
        }

        private void UserPanel_MouseEnter(object sender, EventArgs e)
        {
            UserControlsPanel.BackColor = Color.FromArgb(14, 20, 44);
        }

        private void UserPanel_MouseLeave(object sender, EventArgs e)
        {
            UserControlsPanel.BackColor = Color.FromArgb(24, 30, 54);
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        // Выход из мейнформы
        private void ExitButton_Click(object sender, EventArgs e)
        {
            Hide();
            var LoginForm = new Login();
            LoginForm.Show();
        }

        private void UserPanel_Resize(object sender, EventArgs e)
        {
            UserLabel.ToCenter(UserPanel);
        }

        // масштабируемость канбан доски
        private void ResizeTable()
        {
            TableLayoutPanel.RowStyles[0].SizeType = SizeType.AutoSize;

            // Колонки
            foreach (ColumnStyle column in TableLayoutPanel.ColumnStyles)
            {
                column.SizeType = SizeType.Percent;
                column.Width = 100 / TableLayoutPanel.ColumnCount;
            }

            // Строки
            foreach (var row in TableLayoutPanel.RowStyles.Cast<RowStyle>().ToList().Skip(1))
            {
                row.SizeType = SizeType.Percent;
                row.Height = 100 / TableLayoutPanel.RowCount - 1;
            }

            TableLayoutPanel.Controls.OfType<TicketPanel>().ToList().ForEach(x =>
            {
                x.Width = TableLayoutPanel.Width / TableLayoutPanel.ColumnCount;
                if (TableLayoutPanel.GetCellPosition(x).Row != 0) x.Height = TableLayoutPanel.Height / TableLayoutPanel.RowCount;
            });
        }

        private void CalendarPanel_Resize(object sender, EventArgs e)
        {
            CalendarLabel.ToCenter(CalendarPanel);
            CalendarLabel.MaximumSize = CalendarPanel.Size;
        }

        // Дальше идут три события, связанные с DRAG AND DROP
        private void TableLayoutPanel_DragEnter(object sender, DragEventArgs e)
        {

        }

        private void TableLayoutPanel_DragDrop(object sender, DragEventArgs e)
        {

        }

        private void TableLayoutPanel_DragLeave(object sender, EventArgs e)
        {

        }
    }
}

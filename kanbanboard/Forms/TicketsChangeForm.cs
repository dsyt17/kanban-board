﻿using System;
using System.Windows.Forms;
using kanbanboard.Classes;
using kanbanboard.Controls;

namespace kanbanboard.Forms
{
    public partial class TicketsChangeForm : Form
    {
        public new MainForm Owner { get; }
        private readonly TicketPanel _ticket;

        public TicketsChangeForm(MainForm owner, TicketPanel ticket)
        {
            Owner = owner;
            _ticket = ticket;

            InitializeComponent();

            Resize += (_, _) => ChangingPanel.ToCenter(this);

            KeyDown += (_, a) =>
            {
                if (a.KeyValue == (int)Keys.Enter || a.KeyValue == (int)Keys.Escape) SaveChangesButton.PerformClick();
            };

            var toolTipForProjectNames = new ToolTip { AutomaticDelay = 100 };
            toolTipForProjectNames.SetToolTip(ChangingPeopleLabel, "Формат ввода строго через пробел: Имя1 Имя2 Имя3...");
            toolTipForProjectNames.SetToolTip(ChangingPeopleLabel, "Формат ввода строго через пробел: Имя1 Имя2 Имя3...");
        }

        // Изменяем при нажатии на кнопку сохранения
        private void SaveChangesButton_Click(object sender, EventArgs e) => Close();

        // Показываем значения лейблов тикета
        private void TicketsChangeForm_Load(object sender, EventArgs e)
        {
            ChangingTitleTextBox.Text = _ticket.Title.Text;
            ChangingTicketTextBox.Text = _ticket.Ticket.Text;
            ChangingPeopleTextBox.Text = _ticket.People.Text;
        }

        // Сохранить при закрытии
        private void TicketsChangeForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _ticket.Title.Text = ChangingTitleTextBox.Text;
            _ticket.Ticket.Text = ChangingTicketTextBox.Text;
            _ticket.People.Text = ChangingPeopleTextBox.Text;
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BoggleClient.Game
{
    public partial class GameView : Form, IGameView
    {
        public GameView()
        {
            InitializeComponent();
        }

        public event Action<string> AddWord;
        public event Action CancelPushed;

        public void GenerateLabels(string DiceConfig)
        {
            throw new NotImplementedException();
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpreadsheetGUI;

namespace ControllerTester
{
    public class SimpleView : IView
    {
        /// <summary>
        /// Called when the user creates a new Spreadsheet.
        /// </summary>
        public event FileOperation NewFile;
        public void OnNewFile()
        {

        }

        /// <summary>
        /// Called when the user opens a Spreadsheet.
        /// </summary>
        public event FileOperation OpenFile;
        public void OnOpenFile()
        {

        }

        /// <summary>
        /// Called when the user saves a Spreadsheet.
        /// </summary>
        public event FileOperation SaveFile;
        public void OnSaveFile()
        {

        }

        /// <summary>
        /// Called when the user closes a Spreadsheet.
        /// </summary>
        public event Close CloseFile;
        public void OnCloseFile()
        {

        }

        /// <summary>
        /// Called when the user modifies the contents of a cell in a Spreadsheet.
        /// </summary>
        public event ChangeContent SetContents;
        public void OnSetContents()
        {

        }
    }
}

﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using MaterialDesignExtensions.Controllers;

namespace MaterialDesignExtensions.Controls
{
    public abstract class BaseFileControl : FileSystemControl
    {
        public static RoutedCommand SelectFileCommand = new RoutedCommand();

        public static readonly RoutedEvent FileSelectedEvent = EventManager.RegisterRoutedEvent(
            nameof(FileSelected), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(BaseFileControl));

        public event RoutedEventHandler FileSelected
        {
            add
            {
                AddHandler(FileSelectedEvent, value);
            }

            remove
            {
                RemoveHandler(FileSelectedEvent, value);
            }
        }

        public static readonly DependencyProperty CurrentFileProperty = DependencyProperty.Register(
                nameof(CurrentFile),
                typeof(string),
                typeof(BaseFileControl),
                new PropertyMetadata(null, CurrentFileChangedHandler));

        public string CurrentFile
        {
            get
            {
                return (string)GetValue(CurrentFileProperty);
            }

            set
            {
                SetValue(CurrentFileProperty, value);
            }
        }

        public BaseFileControl()
            : base()
        {
            CommandBindings.Add(new CommandBinding(SelectFileCommand, SelectFileCommandHandler));
        }

        private void SelectFileCommandHandler(object sender, ExecutedRoutedEventArgs args)
        {
            FileSelectedEventArgs eventArgs = new FileSelectedEventArgs(FileSelectedEvent, this, m_controller.CurrentFile);
            RaiseEvent(eventArgs);
        }

        private static void CurrentFileChangedHandler(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            (obj as BaseFileControl)?.CurrentFileChangedHandler(args.NewValue as string);
        }

        protected abstract void CurrentFileChangedHandler(string newCurrentFile);

        protected override void ControllerPropertyChangedHandler(object sender, PropertyChangedEventArgs args)
        {
            if (sender == m_controller)
            {
                if (args.PropertyName == nameof(FileSystemController.DirectoriesAndFiles))
                {
                    List<FileSystemInfo> items = m_controller.DirectoriesAndFiles;
                    m_fileSystemEntryItemsListBox.ItemsSource = items;

                    if (items != null && items.Any())
                    {
                        m_fileSystemEntryItemsListBox.ScrollIntoView(items[0]);
                    }

                    UpdateListVisibility();
                }
                else if (args.PropertyName == nameof(FileSystemController.CurrentFile))
                {
                    if (m_controller.CurrentFile != null)
                    {
                        CurrentFile = m_controller.CurrentFile.FullName;
                    }
                    else
                    {
                        CurrentFile = null;
                    }
                }
            }

            base.ControllerPropertyChangedHandler(sender, args);
        }

        protected override IEnumerable GetFileSystemEntryItems()
        {
            return m_controller.DirectoriesAndFiles;
        }
    }

    public class FileSelectedEventArgs : RoutedEventArgs
    {
        public FileInfo FileInfo { get; }

        public string File
        {
            get
            {
                return FileInfo?.FullName;
            }
        }

        public FileSelectedEventArgs(RoutedEvent routedEvent, object source, FileInfo fileInfo)
            : base(routedEvent, source)
        {
            FileInfo = fileInfo;
        }
    }
}

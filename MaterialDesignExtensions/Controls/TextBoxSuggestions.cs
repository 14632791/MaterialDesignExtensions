﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;

using MaterialDesignExtensions.Controllers;
using MaterialDesignExtensions.Model;

namespace MaterialDesignExtensions.Controls
{
    /// <summary>
    /// A decorator control to add some kind of autocomplete features to a default TextBox.
    /// </summary>
    [ContentProperty(nameof(TextBox))]
    public class TextBoxSuggestions : Control
    {
        private static readonly string SuggestionItemsControlName = "suggestionItemsControl";
        private static readonly string SuggestionItemsPopupName = "suggestionItemsPopup";

        /// <summary>
        /// Internal command used by the XAML template (public to be available in the XAML template). Not intended for external usage.
        /// </summary>
        public static readonly RoutedCommand SelectSuggestionItemCommand = new RoutedCommand();

        /// <summary>
        /// The TextBox to decorate.
        /// </summary>
        public static readonly DependencyProperty TextBoxProperty = DependencyProperty.Register(
            nameof(TextBox), typeof(TextBox), typeof(TextBoxSuggestions), new PropertyMetadata(null, TextBoxChangedHandler));

        /// <summary>
        /// The TextBox to decorate.
        /// </summary>
        public TextBox TextBox
        {
            get
            {
                return (TextBox)GetValue(TextBoxProperty);
            }

            set
            {
                SetValue(TextBoxProperty, value);
            }
        }

        /// <summary>
        /// A source for providing the suggestions.
        /// </summary>
        public static readonly DependencyProperty TextBoxSuggestionsSourceProperty = DependencyProperty.Register(
            nameof(TextBoxSuggestionsSource), typeof(ITextBoxSuggestionsSource), typeof(TextBoxSuggestions), new PropertyMetadata(null, TextBoxSuggestionsSourceChangedHandler));

        /// <summary>
        /// A source for providing the suggestions.
        /// </summary>
        public ITextBoxSuggestionsSource TextBoxSuggestionsSource
        {
            get
            {
                return (ITextBoxSuggestionsSource)GetValue(TextBoxSuggestionsSourceProperty);
            }

            set
            {
                SetValue(TextBoxSuggestionsSourceProperty, value);
            }
        }

        private Popup m_suggestionItemsPopup;
        private ItemsControl m_suggestionItemsControl;

        private AutocompleteController m_autocompleteController;

        static TextBoxSuggestions()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TextBoxSuggestions), new FrameworkPropertyMetadata(typeof(TextBoxSuggestions)));
        }

        /// <summary>
        /// Creates a new <see cref="TextBoxSuggestions" />.
        /// </summary>
        public TextBoxSuggestions()
            : base()
        {
            m_suggestionItemsPopup = null;
            m_suggestionItemsControl = null;

            m_autocompleteController = new AutocompleteController() { AutocompleteSource = TextBoxSuggestionsSource };

            CommandBindings.Add(new CommandBinding(SelectSuggestionItemCommand, SelectSuggestionItemCommandHandler));

            Loaded += LoadedHandler;
            Unloaded += UnloadedHandler;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            m_suggestionItemsPopup = Template.FindName(SuggestionItemsPopupName, this) as Popup;

            m_suggestionItemsControl = Template.FindName(SuggestionItemsControlName, this) as ItemsControl;
        }

        private void LoadedHandler(object sender, RoutedEventArgs args)
        {
            if (m_autocompleteController != null)
            {
                m_autocompleteController.AutocompleteItemsChanged += AutocompleteItemsChangedHandler;
            }

            if (TextBox != null)
            {
                // first remove the event handler to prevent multiple registrations
                TextBox.TextChanged -= TextBoxTextChangedHandler;

                // and then set the event handler
                TextBox.TextChanged += TextBoxTextChangedHandler;
            }
        }

        private void UnloadedHandler(object sender, RoutedEventArgs args)
        {
            if (m_autocompleteController != null)
            {
                m_autocompleteController.AutocompleteItemsChanged -= AutocompleteItemsChangedHandler;
            }

            if (TextBox != null)
            {
                TextBox.TextChanged -= TextBoxTextChangedHandler;
            }
        }

        private void SelectSuggestionItemCommandHandler(object sender, ExecutedRoutedEventArgs args)
        {
            if (TextBox != null)
            {
                TextBox.Text = args.Parameter as string;
            }
        }

        private static void TextBoxChangedHandler(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            (obj as TextBoxSuggestions)?.TextBoxChangedHandler(args.OldValue as TextBox, args.NewValue as TextBox);
        }

        private void TextBoxChangedHandler(TextBox oldTextBox, TextBox newTextBox)
        {
            if (oldTextBox != null)
            {
                oldTextBox.TextChanged -= TextBoxTextChangedHandler;
            }

            if (newTextBox != null)
            {
                newTextBox.TextChanged += TextBoxTextChangedHandler;
            }
        }

        private void TextBoxTextChangedHandler(object sender, TextChangedEventArgs args)
        {
            if (sender == TextBox)
            {
                m_autocompleteController?.Search(TextBox.Text);
            }
        }

        private static void TextBoxSuggestionsSourceChangedHandler(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            (obj as TextBoxSuggestions)?.TextBoxSuggestionsSourceChangedHandler(args.NewValue as ITextBoxSuggestionsSource);
        }

        private void TextBoxSuggestionsSourceChangedHandler(ITextBoxSuggestionsSource textBoxSuggestionsSource)
        {
            if (m_autocompleteController != null)
            {
                m_autocompleteController.AutocompleteSource = textBoxSuggestionsSource;
            }
        }

        private void AutocompleteItemsChangedHandler(object sender, AutocompleteItemsChangedEventArgs args)
        {
            Dispatcher.Invoke(() =>
            {
                SetSuggestionItems(args.Items);
            });
        }

        private void SetSuggestionItems(IEnumerable suggestionItems)
        {
            if (m_suggestionItemsControl != null)
            {
                if (suggestionItems != null)
                {
                    m_suggestionItemsControl.ItemsSource = suggestionItems;
                }
                else
                {
                    m_suggestionItemsControl.ItemsSource = null;
                }
            }
        }
    }
}

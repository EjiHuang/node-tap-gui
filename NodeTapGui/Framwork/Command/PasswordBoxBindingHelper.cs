using System;
using System.Windows;
using System.Windows.Controls;

/*
	[WPF]实现密码框的密码绑定 by:周银辉
	http://www.cnblogs.com/zhouyinhui/archive/2009/08/27/1554943.html
*/
namespace Command
{
	public static class PasswordBoxBindingHelper
	{
		public static bool GetIsPasswordBindingEnabled(DependencyObject obj)
		{
			return (bool)obj.GetValue(IsPasswordBindingEnabledProperty);
		}

		public static void SetIsPasswordBindingEnabled(DependencyObject obj, bool value)
		{
			obj.SetValue(IsPasswordBindingEnabledProperty, value);
		}

		public static readonly DependencyProperty IsPasswordBindingEnabledProperty =
			DependencyProperty.RegisterAttached("IsPasswordBindingEnabled", typeof(bool),
			typeof(PasswordBoxBindingHelper),
			new UIPropertyMetadata(false, OnIsPasswordBindingEnabledChanged));

		private static void OnIsPasswordBindingEnabledChanged(DependencyObject obj,
															  DependencyPropertyChangedEventArgs e)
		{
			var passwordBox = obj as PasswordBox;

			if (passwordBox != null)
			{
				passwordBox.PasswordChanged -= PasswordBoxPasswordChanged;

				if ((bool)e.NewValue)
				{
					passwordBox.PasswordChanged += PasswordBoxPasswordChanged;
				}

			}
		}

		//when the passwordBox's password changed, update the buffer
		static void PasswordBoxPasswordChanged(object sender, RoutedEventArgs e)
		{
			var passwordBox = (PasswordBox)sender;

			if (!String.Equals(GetBindedPassword(passwordBox), passwordBox.Password))
			{
				SetBindedPassword(passwordBox, passwordBox.Password);
			}
		}


		public static string GetBindedPassword(DependencyObject obj)
		{
			return (string)obj.GetValue(BindedPasswordProperty);
		}


		public static void SetBindedPassword(DependencyObject obj, string value)
		{
			obj.SetValue(BindedPasswordProperty, value);
		}

		public static readonly DependencyProperty BindedPasswordProperty =
			DependencyProperty.RegisterAttached("BindedPassword", typeof(string),
			typeof(PasswordBoxBindingHelper),
			new UIPropertyMetadata(string.Empty, OnBindedPasswordChanged));

		//when the buffer changed, upate the passwordBox's password
		private static void OnBindedPasswordChanged(DependencyObject obj,
													DependencyPropertyChangedEventArgs e)
		{
			var passwordBox = obj as PasswordBox;
			if (passwordBox != null)
			{

				passwordBox.Password = e.NewValue == null ? string.Empty : e.NewValue.ToString();
			}
		}

	}
}

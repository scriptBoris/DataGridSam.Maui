﻿using Sample.Core;
using Sample.Views;

namespace Sample;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		DataCollector.GenerateUsers();
		MainPage = new NavigationPage(new MainPage());
		//MainPage = new DataGridSamplePage();
    }
}

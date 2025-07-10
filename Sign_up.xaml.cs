using System;
using System.Windows;
using System.Windows.Input;
using DotNetEnv; // .env 파일을 불러오기 위해 필요
using MySql.Data.MySqlClient; // MariaDB와의 연결을 위해 필요
using Project;
using user_client.Model;

namespace WpfApp
{
    public partial class Sign_up : Window // Sign_up.xaml과 짝을 이루는 코드 숨김 파일
    {
        public Sign_up() // 생성자: 이 윈도우가 처음 생성될 때 호출됨
        {
            InitializeComponent(); // XAML에 정의된 UI 요소들을 초기화해주는 함수
        }

    }
}

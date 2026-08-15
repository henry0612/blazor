/// <summary>
/// サンプルを出力します
/// </summary>
private void OutputSample()
{
    // 帳票クリエータのインスタンスを生成します
    var creator = new CnDocCreator();
 
	try
	{		
		// 設定ファイルをロードします
		creator.LoadConfiguration("sample.dcx");
 
		// 設定ファイルクラスの内容を変更します
		// ここではText2フィールドのテキスト置換テンプレートで使用するデータを
		// 外部パラメータ（ユーザ定義）「キー：Text2、値：データ」として渡しています
		// （外部パラメータの設定はセキュリティを考慮して非推奨になっていますが使用可能です）
		var userDefined = new Dictionary<string, string>();
		userDefined.Add("Text2", "データ");
		creator.Configuration.UserDefined = userDefined;
 
		// クリエータを初期化します
		creator.Initialize();
		// 設定ファイル中に記述がない処理の追加やプロパティの変更を行います
		// ここでは出力帳票名を「Hello, World」に変更しています
		creator.CnJobs["sample"].OutputName = "Hello, World";
		// 出力を行います
		creator.Output();
		// 出力したファイル名を表示します
		MessageBox.Show(creator.DocumentFileNames["sample"]);
	}
	catch (CnException cex)
	{
		// シーオーリポーツ内で発生した例外を捕捉します
		MessageBox.Show(cex.Message);
	}
	catch (Exception ex)
	{
		// 想定外の例外を捕捉します
		MessageBox.Show(ex.Message);
	}
    finally
    {
		// クリエータの開放処理を行います。
		creator.Dispose();
    }
}
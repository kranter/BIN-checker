using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;


// Лубенец Илья Игоревич (aka. Oopsie Doopsie)

namespace Request1
{
    class Program
    {
        //Вызываю демона для работы с HTTP
        private static readonly HttpClient client = new HttpClient();

        //Основная программа
        static async Task Main(string[] args)
        {
            string BIN = null; // Сюда запишем нужный BIN для запроса
            string[] Files = Directory.GetFiles(@"Temp/", "BIN-*.txt", SearchOption.TopDirectoryOnly);  // Вместе с маской ищу файлик

            foreach (string file in Files)
            {
                BIN = GetCorrectBIN(file.ToCharArray());
                //Запрос на сайт для чека банковской карты
                try
                {
                    //Параметры для POST-запроса
                    var FormData = new Dictionary<string, string>
                    {
                    { "bin-input", BIN},
                    { "g-recaptcha-response", "" }
                    };

                    // Кодирую данные для запроса
                    var qFormData = new FormUrlEncodedContent(FormData);
                    //Жду ответ
                    var QuerryResponse = await client.PostAsync("https://psm7.com/bin/worker.php", qFormData);
                    //Перевожу в стрингу 
                    var StringResponse = await QuerryResponse.Content.ReadAsStringAsync();
                    //Запуск функции записи в файл
                    WriteStringToFile(ref StringResponse, ref BIN);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error:{ex.Message}");
                }
            }
        }
//---------------------------------------------------------------------------------------------------------------------------------------
//---------------------------------------------------Фиксим BIN до нужного---------------------------------------------------------------
//---------------------------------------------------------------------------------------------------------------------------------------

        //Беру из названия файла (с шириной расширения 3, ex. .txt, .dat...) номер BIN
        static string GetCorrectBIN(char[] cFileName)
        {
            string BIN = null;
            for(int i = cFileName.Length - 10; i < cFileName.Length - 4; i++)
            {
                BIN += cFileName[i].ToString();
            }

            return BIN;
        }

//---------------------------------------------------------------------------------------------------------------------------------------
//---------------------------------------------------Пишем в новый файл и удаляем старый-------------------------------------------------
//---------------------------------------------------------------------------------------------------------------------------------------

        //Записываю в файл, который лежит рядом с .exe, в формате _BIN-XXXXXX.txt
        static void WriteStringToFile(ref string Line, ref string BIN_original) {

            //Заранее говорю, что проверки на существующий файл не будет, потому что нужно только свежая информация))

            //Даю удобное название файлу
            string BIN_sw = $"_BIN-{BIN_original}.txt";
            
            try
            {
                //Создаю писателя, который всё занесёт в файл и сам закроеутся.
                using (StreamWriter sw = new StreamWriter($"{BIN_sw}", false, System.Text.Encoding.Default))
                {
                    sw.WriteLine(Line);
                    sw.Close();
                }
                File.Delete($"Temp/BIN-{BIN_original}.txt");
            }
            catch(Exception ex)
            {
                //Вывод ошибкив консоль, если она есть
                Console.WriteLine($"Error: {ex.Message}");
            }

        }
    }
}

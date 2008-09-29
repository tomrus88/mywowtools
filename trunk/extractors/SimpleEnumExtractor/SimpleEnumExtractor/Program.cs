using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Win32;
using WoWReader;
using System.CodeDom;
using Microsoft.CSharp;
using System.CodeDom.Compiler;

namespace SimpleEnumExtractor {
	class Program {
		static GenericReader gr = null;
		static TextReader tr = null;

		static string stream_string = String.Empty;

		static void Main(string[] args) {
			// Ищем файл wow.exe сначала в текущей папке,
			// затем лезем в реестр и смотрим папку там.
			// TODO: взять путь к wow.exe из аргументов ком. строки
			var wowPath = "WoW.exe";
			if(!File.Exists(wowPath)) {
				wowPath = (string)Registry.GetValue(@"HKEY_CURRENT_USER\Software\Blizzard Entertainment\World of Warcraft",
					"InstallPath", null);
				wowPath = Path.Combine(wowPath ?? string.Empty, "WoW.exe");
			}

			var wow_exe = new FileInfo(wowPath);
			if(!wow_exe.Exists) {
				Console.WriteLine("File {0} not found!", wow_exe.Name);
			}
			else {
				Console.WriteLine("File {0} found in {1}", wow_exe.Name, wow_exe.DirectoryName);
				Console.WriteLine("Extracting...");
				var stream = wow_exe.Open(FileMode.Open, FileAccess.Read);
				gr = new GenericReader(stream, Encoding.ASCII);
				tr = new StreamReader(stream, Encoding.ASCII);

				stream_string = tr.ReadToEnd();

				ExtractEnum("ChatMsg", "CHAT_MSG_BATTLEGROUND_LEADER", "CHAT_MSG_ADDON", "{0}", -1);
				ExtractEnum("ResponseCodes", "CHAR_NAME_DECLENSION_DOESNT_MATCH_BASE_NAME",
					"RESPONSE_SUCCESS");
				ExtractEnum("ChatNotify", "VOICE_OFF", "YOU_CHANGED", "CHAT_{0}_NOTICE", 0);
				ExtractEnum("OpCodes", "NUM_MSG_TYPES", "MSG_NULL_ACTION");
				ExtractEnum("SpellFailedReason", "SPELL_FAILED_UNKNOWN",
					"SPELL_FAILED_AFFECTING_COMBAT");
				ExtractEnum("EnchantConditions", "ENCHANT_CONDITION_REQUIRES",
					"ENCHANT_CONDITION_EQUAL_VALUE");

				tr.Close();
				gr.Close();
				stream.Close();
				Console.WriteLine("Done");
			}
			Console.Write("Press any key to exit...");
			Console.ReadKey();
		}

		static void ExtractEnum(string name, string start, string end, string format, int index) {
			try {
				Console.Write("  Extracting {0,-61}", name);
				DumpToFile(name, GetNames(start, end, format), index);
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("done");
			}
			catch {
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("error");
			}
			finally {
				Console.ResetColor();
			}
		}

		static void ExtractEnum(string name, string start, string end) {
			ExtractEnum(name, start, end, "{0}", 0);
		}

		static string[] GetNames(string start, string end, string format) {
			gr.BaseStream.Position = FindStartPos(start);
			var names = new List<string>();
			var name = string.Empty;
			do {
				name = gr.ReadStringNull();
				names.Add(string.Format(format, name));
				FindNextPos(gr);
			} while(name != end);
			names.Reverse();
			return names.ToArray();
		}

		static string[] GetNames(string start, string end) {
			return GetNames(start, end, "{0}");
		}

		static void DumpEnumToFileOld(string enumName, string[] names, int first) {
			using(var writer = new StreamWriter(enumName + ".h")) {
				writer.WriteLine("enum " + enumName);
				writer.WriteLine("{");

				foreach(string str in names) {
					writer.WriteLine("    {0,-45} = 0x{1:X2},", str, first);
					first++;
				}

				writer.WriteLine("};");
			}
		}

		static void DumpToFile(string enumName, string[] names, int first) {
			// TODO: Необходимо задавать провайдер из параметров командной строки
			// по умолчанию брать Cpp провайдер
			var @enum = new CodeTypeDeclaration(enumName) {
				IsEnum = true,
			};

			foreach(var name in names) {
				var field = new CodeMemberField(string.Empty, name) {
					InitExpression = new CodePrimitiveExpression(first),
				};
				@enum.Members.Add(field);
				first++;
			}

			var codeProvider = CodeDomProvider.CreateProvider("Cpp");
			var options = new CodeGeneratorOptions() {
				BlankLinesBetweenMembers = false,
			};

			using(var writer = new StreamWriter(enumName + "." + codeProvider.FileExtension)) {
				codeProvider.GenerateCodeFromType(@enum, writer, options);
			}
		}

		static void DumpToFile(string enumName, string[] names) {
			DumpToFile(enumName, names, 0);
		}

		static int FindStartPos(string name) {
			return stream_string.IndexOf(name + '\0');
		}

		static int FindNextPos(GenericReader reader) {
			while(reader.PeekChar() == 0x00) {
				reader.BaseStream.Position++;
			}
			return (int)reader.BaseStream.Position;
		}
	}
}

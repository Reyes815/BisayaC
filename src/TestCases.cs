namespace Bisaya__
{
    internal class TestCases
    {
        class TestCase
        {
            public string? Code { get; set; }
            public string? ExpectedOutput { get; set; }
        }

        public static void RunTests()
        {
            var tests = InitializeTests();
            var results = new List<string>();
            int passCount = 0;
            int failCount = 0;
            bool printToConsole = false;

            TextWriter originalConsoleOut = Console.Out;
            foreach (var test in tests)
            {
                try
                {
                    var tokens = Lexer.Tokenize(test.Code);
                    var parser = new Parser(tokens);
                    var ast = parser.Parse();

                    using (var sw = new StringWriter())
                    {
                        Console.SetOut(sw);

                        var interpreter = new InterpreterClass();
                        interpreter.Interpret(ast);

                        Console.Out.Flush();

                        var output = sw.ToString().Trim();
                        bool pass = output.Equals(test.ExpectedOutput);
                        results.Add($"\nExpected: {test.ExpectedOutput}\nActual: {output}\nResult: {(pass ? "PASS" : "FAIL")}\n");

                        if (pass) passCount++;
                        else failCount++;

                        if (printToConsole)
                        {
                            Console.SetOut(originalConsoleOut);
                            Console.WriteLine(output);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.SetOut(originalConsoleOut);
                    results.Add($"\nExpected: {test.ExpectedOutput}\nActual: {ex.Message}\nResult: FAIL\n");
                    failCount++;
                }
            }

            Console.SetOut(originalConsoleOut);
            int i = 1;
            foreach (var result in results)
            {
                Console.WriteLine($"TEST CASE {i}: {result}");
                i++;
            }

            Console.WriteLine($"\nTotal Test Cases: {tests.Count}\nPassed: {passCount}\nFailed: {failCount}");
        }

        private static List<TestCase> InitializeTests()
        {
            return new List<TestCase>
            {
                new TestCase {
                    Code = @"SUGOD
                                 MUGNA NUMERO xyz, abc=100
                                 xyz= ((abc*5)/10 + 10) * -1
                                 IPAKITA: [[] & xyz & []]
                             KATAPUSAN",
                    ExpectedOutput = "[-60]"
                },
                new TestCase {
                    Code = @"SUGOD
                                 MUGNA NUMERO x, y, z=5
                                 MUGNA LETRA a_1='n'
                                 MUGNA TINUOD t=""OO""
                                 x=y=4
                                 a_1='c'
                                 -- this is a comment
                                 IPAKITA: x & t & z & $ & a_1 & [#] & ""last""
                             KATAPUSAN",
                    ExpectedOutput = "4OO5\nc#last"
                },
                new TestCase {
                    Code = @"SUGOD
                                MUGNA NUMERO a=100, b=200, c=300
                                MUGNA TINUOD d=""DILI""
                                d = (a < b UG c <> 200)
                                IPAKITA: d
                             KATAPUSAN",
                    ExpectedOutput = "OO"
                },
                new TestCase {
                    Code = @"SUGOD
                                 MUGNA NUMERO a=100, b=200, c=300
                                 c = (-a + 1) * -2
                                 IPAKITA: c
                             KATAPUSAN",
                    ExpectedOutput = "198"
                },
                new TestCase {
                    Code = @"SUGOD
                                 MUGNA NUMERO a=2, b=9, c=0
                                 c = a + b
                                 IPAKITA: c
                             KATAPUSAN",
                    ExpectedOutput = "11"
                },
                new TestCase {
                    Code = @"SUGOD
                                MUGNA NUMERO i = 0
                                i++
                                IPAKITA: i
                             KATAPUSAN",
                    ExpectedOutput = "1"
                },
                new TestCase {
                    Code = @"SUGOD
                                MUGNA TIPIK x = 10.0
                                x = x * 3
                                IPAKITA: x
                             KATAPUSAN",
                    ExpectedOutput = "30.0"
                },
                new TestCase {
                    Code = @"SUGOD
                                MUGNA NUMERO a = 10
                                a = a % 3
                                IPAKITA: a
                             KATAPUSAN",
                    ExpectedOutput = "1"
                },
                new TestCase {
                    Code = @"SUGOD
                                MUGNA NUMERO a = 5
                                a = a + 5
                                a = a - 2
                                a = a * 2
                                IPAKITA: a
                             KATAPUSAN",
                    ExpectedOutput = "16"
                },
                new TestCase {
                    Code = @"SUGOD
                                MUGNA NUMERO i = 1
                                IPAKITA: i++ & "" "" & i
                             KATAPUSAN",
                    ExpectedOutput = "2 1"
                },
                new TestCase {
                    Code = @"SUGOD
                                MUGNA NUMERO count = 5
                                count = count - 1
                                count = count - 1
                                IPAKITA: count
                             KATAPUSAN",
                    ExpectedOutput = "3"
                },
                new TestCase {
                    Code = @"SUGOD
                                MUGNA NUMERO x = 10, y = 20
                                MUGNA TINUOD result = (x * y - 5 > 150) UG (x + y == 30)
                                IPAKITA: result
                             KATAPUSAN",
                    ExpectedOutput = "OO"
                },
                new TestCase {
                    Code = @"SUGOD
                                MUGNA TINUOD a = ""OO"", b = ""DILI"", c = ""OO""
                                IPAKITA: (a UG b) O (b O c)
                             KATAPUSAN",
                    ExpectedOutput = "OO"
                },
                new TestCase {
                    Code = @"SUGOD
                                MUGNA NUMERO score = 85
                                KUNG (score > 90)
                                PUNDOK{
                                    IPAKITA: ""A""
                                }
                                KUNG DILI (score > 80)
                                PUNDOK{
                                    IPAKITA: ""B""
                                }
                                KUNG WALA
                                PUNDOK{
                                    IPAKITA: ""C""
                                }
                             KATAPUSAN",
                    ExpectedOutput = "B"
                },
                new TestCase {
                    Code = @"SUGOD
                                MUGNA TIPIK pi = 3.14
                                MUGNA NUMERO radius = 5
                                MUGNA TIPIK area = pi * (radius * radius)
                                IPAKITA: area
                             KATAPUSAN",
                    ExpectedOutput = "78.5"
                },
                new TestCase {
                    Code = @"SUGOD
                                IPAKITA: ""Resulta:"" & $ & ""Katapusan sa Linya""
                             KATAPUSAN",
                    ExpectedOutput = "Resulta:\nKatapusan sa Linya"
                },
                new TestCase {
                    Code = @"SUGOD
                                MUGNA NUMERO _1a = 10, a1 = 20
                                IPAKITA: _1a & a1
                             KATAPUSAN",
                    ExpectedOutput = "1020"
                },
                new TestCase {
                    Code = @"SUGOD
                                MUGNA LETRA a = '['
                                IPAKITA: ""[["" & a & ""]]""
                             KATAPUSAN",
                    ExpectedOutput = "[[[]]"
                },
                new TestCase {
                    Code = @"SUGOD
                                MUGNA NUMERO x = 15
                                KUNG (x > 10)
                                PUNDOK{
                                    KUNG (x < 20)
                                    PUNDOK{
                                        IPAKITA: ""Tunga sa 10 ug 20""
                                    }
                                }
                             KATAPUSAN",
                    ExpectedOutput = "Tunga sa 10 ug 20"
                },
                new TestCase {
                    Code = @"SUGOD
                                MUGNA TINUOD x = DILI ""OO""
                                IPAKITA: x
                             KATAPUSAN",
                    ExpectedOutput = "DILI"
                },
                new TestCase {
                    Code = @"SUGOD
                                MUGNA NUMERO a = 5
                                MUGNA TINUOD b = (a == 5)
                                IPAKITA: b UG ""OO""
                             KATAPUSAN",
                    ExpectedOutput = "OO"
                },
                new TestCase {
                    Code = @"SUGOD
                                MUGNA NUMERO num = 5
                                MUGNA LETRA ch = 'A'
                                IPAKITA: ch & num
                             KATAPUSAN",
                    ExpectedOutput = "A5"
                },
                new TestCase {
                    Code = @"SUGOD
                                MUGNA NUMERO result = 10 + 2 * 5
                                IPAKITA: result
                             KATAPUSAN",
                    ExpectedOutput = "20"
                },
                new TestCase {
                    Code = @"SUGOD
                                MUGNA TIPIK x = 3.5, y = 2.0
                                MUGNA NUMERO result = x * y
                                IPAKITA: result
                             KATAPUSAN",
                    ExpectedOutput = "7"
                },
                new TestCase {
                    Code = @"SUGOD
                                IPAKITA: ""Ang resulta kay "" & (5 > 3)
                             KATAPUSAN",
                    ExpectedOutput = "Ang resulta kay OO"
                },
                new TestCase {
                    Code = @"SUGOD
                                IPAKITA: ""Huy Kalibutan[$]""
                             KATAPUSAN",
                    ExpectedOutput = "Huy Kalibutan[$]"
                },
                new TestCase {
                    Code = @"SUGOD
                                MUGNA TIPIK num = 3.14159
                                IPAKITA: num
                             KATAPUSAN",
                    ExpectedOutput = "3.14159"
                },
                new TestCase {
                    Code = @"SUGOD
                                MUGNA TINUOD a = ""OO"", b = ""DILI""
                                IPAKITA: (DILI a) UG b O (a UG DILI b)
                             KATAPUSAN",
                    ExpectedOutput = "OO"
                },
                new TestCase {
                    Code = @"SUGOD
                                MUGNA TIPIK a = 5.5, b = 2.2
                                IPAKITA: (a * b) / (a - b) + 100
                             KATAPUSAN",
                    ExpectedOutput = "103.666664"
                },
                new TestCase {
                    Code = @"SUGOD
                                IPAKITA: ""--Test ni huy"" --Dapat dili ni makita
                             KATAPUSAN",
                    ExpectedOutput = "--Test ni huy"
                },
                new TestCase {
                    Code = @"SUGOD
                                MUGNA NUMERO score = 75
                                KUNG (score >= 90)
                                PUNDOK{
                                    IPAKITA: ""A""
                                }
                                KUNG DILI (score >= 80)
                                PUNDOK{
                                    IPAKITA: ""B""
                                }
                                KUNG DILI (score >= 70)
                                PUNDOK{
                                    IPAKITA: ""C""
                                }
                                KUNG WALA
                                PUNDOK{
                                    IPAKITA: ""F""
                                }
                             KATAPUSAN",
                    ExpectedOutput = "C"
                },
                new TestCase {
                    Code = @"SUGOD
                                MUGNA NUMERO a = 69, b = 420
                                IPAKITA: a < b
                             KATAPUSAN",
                    ExpectedOutput = "OO"
                },
                new TestCase {
                    Code = @"SUGOD
                                MUGNA TIPIK f = 3.14
                                MUGNA NUMERO i = 2
                                IPAKITA: ""Pi approx: "" & f & "", Multiplier: "" & i
                             KATAPUSAN",
                    ExpectedOutput = "Pi approx: 3.14, Multiplier: 2"
                },
                new TestCase {
                    Code = @"SUGOD
                                MUGNA TIPIK a = 2.5
                                MUGNA NUMERO b = a * 2
                                IPAKITA: b
                             KATAPUSAN",
                    ExpectedOutput = "5"
                },
                new TestCase {
                    Code = @"SUGOD
                                MUGNA LETRA first = 'H', last = 'W'
                                IPAKITA: ""Hello "" & first & ""orld"" & last
                             KATAPUSAN",
                    ExpectedOutput = "Hello HorldW"
                },
                new TestCase {
                    Code = @"SUGOD
                                MUGNA NUMERO i = 1
                                MUGNA TIPIK f = 1.1
                                MUGNA LETRA c = 'c'
                                MUGNA TINUOD b = ""OO""
                                IPAKITA: i & f & c & b
                             KATAPUSAN",
                    ExpectedOutput = "11.1cOO"
                },
                new TestCase {
                    Code = @"SUGOD
                                MUGNA TIPIK f = 5.0
                                MUGNA NUMERO i = 2
                                IPAKITA: f / i
                             KATAPUSAN",
                    ExpectedOutput = "2.5"
                },
                new TestCase {
                    Code = @"SUGOD
                                MUGNA TINUOD a = ""OO"", b = ""OO""
                                KUNG (a O b)
                                PUNDOK{
                                    IPAKITA: ""Either true or false""
                                }
                                KUNG WALA
                                PUNDOK{
                                    IPAKITA: ""Neither""
                                }
                             KATAPUSAN",
                    ExpectedOutput = "Either true or false"
                },
                new TestCase {
                    Code = @"SUGOD
                                MUGNA NUMERO a = 10, b = 20
                                MUGNA TIPIK c = 3.5
                                MUGNA LETRA d = 'x'
                                IPAKITA: a & b & c & d
                             KATAPUSAN",
                    ExpectedOutput = "10203.5x"
                },
                new TestCase {
                    Code = @"SUGOD
                                MUGNA TIPIK f = 1.999
                                MUGNA NUMERO i = f
                                IPAKITA: i
                             KATAPUSAN",
                    ExpectedOutput = "1"
                },
                new TestCase {
                    Code = @"SUGOD
                                MUGNA TINUOD a = ""DILI"", b = ""OO""
                                KUNG (DILI a UG (b O DILI (a UG b)))
                                PUNDOK{
                                    IPAKITA: ""True complex""
                                }
                             KATAPUSAN",
                    ExpectedOutput = "True complex"
                },
                new TestCase {
                    Code = @"SUGOD
                                MUGNA NUMERO a = 10
                                MUGNA TIPIK b = 5.5
                                IPAKITA: a & b
                             KATAPUSAN",
                    ExpectedOutput = "105.5"
                },
                new TestCase {
                    Code = @"SUGOD
                                MUGNA NUMERO kung = 10
                                IPAKITA: kung
                             KATAPUSAN",
                    ExpectedOutput = "10"
                },
                new TestCase {
                    Code = @"SUGOD
                                MUGNA TIPIK f = 0.1 + 0.2
                                IPAKITA: f
                             KATAPUSAN",
                    ExpectedOutput = "0.3"
                },
                new TestCase {
                    Code = @"SUGOD
                                MUGNA NUMERO a = 10
                                MUGNA TINUOD b = (a > 5) UG (a < 15)
                                IPAKITA: b
                             KATAPUSAN",
                    ExpectedOutput = "OO"
                },
                new TestCase
                {
                    Code = @"SUGOD
	                            MUGNA TINUOD test = (10 > 5)
	                            KUNG (test == ""OO"")
	                            PUNDOK{
		                            IPAKITA: ""OO""
	                            }
	                            KUNG WALA
	                            PUNDOK{
		                            IPAKITA: ""DILI""
	                            }
                            KATAPUSAN",
                    ExpectedOutput = "OO"
                },
                new TestCase
                {
                    Code = @"SUGOD
                                MUGNA NUMERO i = 0
                                KUNG (i + 1 < 1)
                                PUNDOK{
                                    IPAKITA: ""i is less than 1 after increment""
                                }
                                KUNG DILI (i + 1 == 1)
                                PUNDOK{
                                    IPAKITA: ""i is equal to 1 after increment""
                                }
                                KUNG WALA
                                PUNDOK{
                                    IPAKITA: ""i is greater than 1 after increment""
                                }
                             KATAPUSAN",
                    ExpectedOutput = "i is equal to 1 after increment"
                },
                new TestCase
                {
                    Code = @"SUGOD
                                MUGNA TINUOD a = ""OO"", b = ""DILI""
                                KUNG (a UG (DILI b O a))
                                PUNDOK{
                                    IPAKITA: ""Complex logic passed""
                                }
                                KUNG WALA
                                PUNDOK{
	                                IPAKITA: ""Complex logic failed""
                                }
                             KATAPUSAN",
                    ExpectedOutput = "Complex logic passed"
                },
                new TestCase
                {
                    Code = @"SUGOD
                                MUGNA NUMERO x
                                x = 5 + 5 * 4 / 2 + (5 * 4 / 2)
                                IPAKITA: x
                             KATAPUSAN",
                    ExpectedOutput = "25"
                },
                new TestCase
                {
                    Code = @"SUGOD
	                            MUGNA NUMERO n = 15
	                            MUGNA NUMERO t,j,i

	                            SAMTANG(t<n)
	                            PUNDOK{
		                            j++
		                            KUNG(j <= 3)
		                            PUNDOK{
			                            IPAKITA: i+1 & "" ""
			                            t++
		                            }
		                            KUNG WALA
		                            PUNDOK{
			                            i=i+3
			                            j=0
		                            }
		                            i++
	                            }
                            KATAPUSAN",
                    ExpectedOutput = "1 2 3 8 9 10 15 16 17 22 23 24 29 30 31"
                },
                new TestCase
                {
                    Code = @"SUGOD
	                            MUGNA NUMERO n
	                            MUGNA NUMERO n1 = 1, n2 = 1
	                            MUGNA NUMERO nth = n1 + n2
	                            MUGNA NUMERO i = 4

	                            n = 10

	                            KUNG (n < 1 O n == 1)
	                            PUNDOK{
		                            IPAKITA: ""Value(n) must be greater than 1""
	                            }
	                            KUNG WALA
	                            PUNDOK{
		                            IPAKITA: ""Fibonacci sequence: "" & n1 & "" "" & n2 & "" ""
	                            }

	                            KUNG (n > 3)
	                            PUNDOK{
		                            SAMTANG (i <= n+1)
		                            PUNDOK{
			                            IPAKITA: nth & "" ""
			                            n1 = n2
			                            n2 = nth
			                            nth = n1 + n2
			                            i++
		                            }	
	                            }
                            KATAPUSAN",
                    ExpectedOutput = "Fibonacci sequence: 1 1 2 3 5 8 13 21 34 55"
                },
                new TestCase
                {
                    Code = @"SUGOD
	                            MUGNA NUMERO n
	                            MUGNA NUMERO factorial = 1, i = 2

	                            n = 5

	                            SAMTANG (i <= n)
	                            PUNDOK{
		                            factorial = factorial * i
		                            i++
	                            }
	                            IPAKITA: ""Factorial of "" & n & "" is "" & factorial
                            KATAPUSAN",
                    ExpectedOutput = "Factorial of 5 is 120"
                },
                new TestCase
                {
                    Code = @"SUGOD
                                MUGNA NUMERO ctr
                                ALANG SA(ctr=1, ctr<=10, ctr++)
                                PUNDOK{
                                    IPAKITA: ctr & ' '
                                }
                            KATAPUSAN",
                    ExpectedOutput = "1 2 3 4 5 6 7 8 9 10"
                },
                new TestCase
                {
                    Code = @"SUGOD
                                MUGNA NUMERO i,j
                                ALANG SA(i=0, i<5, i++)
                                PUNDOK{
                                    ALANG SA(j=0, j<5, j++)
                                    PUNDOK{
                                        KUNG(i == j)
                                        PUNDOK{
                                            IPAKITA: ""Diagonal"" & i & "" ""
                                        }
                                        KUNG(i + j == 4)
                                        PUNDOK{
                                            IPAKITA: ""Diagonal"" & j & "" ""
                                        }
                                    }
                                }
                             KATAPUSAN",
                    ExpectedOutput = "Diagonal0 Diagonal4 Diagonal1 Diagonal3 Diagonal2 Diagonal2 Diagonal1 Diagonal3 Diagonal0 Diagonal4"
                },
                new TestCase {
                    Code = @"SUGOD
                                MUGNA NUMERO i 
                                ALANG SA(i=0, i<10, i++)
                                PUNDOK{
                                    KUNG(i % 2 == 0)
                                    PUNDOK{
                                        IPAKITA: i & "" is even ""
                                    }
                                    KUNG WALA
                                    PUNDOK{
                                        IPAKITA: i & "" is odd ""
                                    }
                                }
                             KATAPUSAN",
                    ExpectedOutput = "0 is even 1 is odd 2 is even 3 is odd 4 is even 5 is odd 6 is even 7 is odd 8 is even 9 is odd"
                },
                new TestCase {
                    Code = @"SUGOD
                                MUGNA NUMERO a
                                ALANG SA(a=0, a<3, a++)
                                PUNDOK{
                                    IPAKITA: a & "" ""
                                }
                             KATAPUSAN",
                    ExpectedOutput = "0 1 2"
                },
                //new TestCase {
                //    Code = @"SUGOD
	               //             ALANG SA(i=5, i>0, i--)
	               //             PUNDOK{
		              //              KUNG(i <> 2)
		              //              PUNDOK{
			             //               IPAKITA: i & "" ""
		              //              }
	               //             }
                //            KATAPUSAN",
                //    ExpectedOutput = "4 3"
                //},
                new TestCase {
                    Code = @"SUGOD
                                MUGNA NUMERO i, j
                                ALANG SA(i=0, i<3, i++)
                                PUNDOK{
                                    ALANG SA(j=0, j<2, j++)
                                    PUNDOK{
                                        IPAKITA: i & j & "" ""
                                    }
                                }
                             KATAPUSAN",
                    ExpectedOutput = "00 01 10 11 20 21"
                },
            };
        }
    }
}

using MasterMind.Exceptions;

namespace MasterMind.Players
{
    public class IAPlayer : Player
    {
        private const int SEQUENCEDIM = 4;
        public enum Role { CRYPTER, DECRYPTER }

        // every time i have e response from the Crypter i need to "prune" the sequence, i need to remove all the element that have not the same Response, so i need a data structure
        // that is enough efficent in removal, i opted for the HashSet (it can't take doplicate and that's fine couse i need all the different Provision of the sequence) 
        private static HashSet<Match.Symbols[]>? _allPossibileCodifiedSequence = null;
        private Role _IArole;
        private HashSet<Match.Symbols[]>? _allPossibleSequenceCopy = null;
        private Match.Symbols[]? _codifiedSequence;

        private void GenerateAllPossibleSequence()
        {
            if (_allPossibileCodifiedSequence == null)
            {
                _allPossibileCodifiedSequence = new();
                Match.Symbols[] matchSybles = Enum.GetValues(typeof(Match.Symbols)).Cast<Match.Symbols>().ToArray();

                // i have to generate all the possibile sequence of 4 elements that can be made from a the symbols

                for (int symbol1 = 0; symbol1 < matchSybles.Length; symbol1++)
                {
                    for (int symbol2 = 0; symbol2 < matchSybles.Length; symbol2++)
                    {
                        for (int symbol3 = 0; symbol3 < matchSybles.Length; symbol3++)
                        {
                            for (int symbol4 = 0; symbol4 < matchSybles.Length; symbol4++)
                            {
                                if (symbol1 == symbol2 || symbol1 == symbol3 || symbol1 == symbol4 || symbol2 == symbol3 || symbol2 == symbol4 || symbol3 == symbol4) continue;
                                _allPossibileCodifiedSequence.Add(new Match.Symbols[] { matchSybles[symbol1], matchSybles[symbol2], matchSybles[symbol3], matchSybles[symbol4] });
                            }
                        }
                    }
                }
            }
        }

        private Match.Symbols[] GenereteCodifiedSequence()
        {
            int randomIteration = new Random(DateTime.Now.Millisecond).Next(8, 100);
            _codifiedSequence = new Match.Symbols[SEQUENCEDIM];
            Match.Symbols[] shuffledSymbols = Enum.GetValues(typeof(Match.Symbols)).Cast<Match.Symbols>().ToArray();

            // shuffling the sequence a random number of time, after i'll take the 4 element from the shuffled array
            for (byte i = 0; i < randomIteration; i++)
            {
                (shuffledSymbols[i % shuffledSymbols.Length], shuffledSymbols[(i + 2) % shuffledSymbols.Length]) = (shuffledSymbols[(i + 2) % shuffledSymbols.Length], shuffledSymbols[i % shuffledSymbols.Length]);
            }

            // choosing if i'll take the sequence from the start or from the end
            if (randomIteration % 2 == 0) _codifiedSequence = shuffledSymbols.Take(SEQUENCEDIM).ToArray();
            else _codifiedSequence = shuffledSymbols.Skip(SEQUENCEDIM).Take(SEQUENCEDIM).ToArray();

            return _codifiedSequence;
        }

        private void PruneSequence(string response)
        {
            if (_allPossibleSequenceCopy == null) throw new Exception();
            foreach (Match.Symbols[] provision in _allPossibleSequenceCopy)
            {
                if (TestProvision(provision) != response) _allPossibleSequenceCopy.Remove(provision);
            }
        }

        private string TestProvision(Match.Symbols[] provision)
        {
            if (_codifiedSequence == null) throw new InvalidMove();
            byte B = 0, R = 0;
            for (byte i = 0; i < provision.Length; i++)
            {
                if (_codifiedSequence[i] == provision[i]) B++;
                else
                {
                    for (byte j = 0; j < _codifiedSequence.Length; j++)
                    {
                        if (provision[i] == _codifiedSequence[j])
                        {
                            R++;
                            break;
                        }
                    }
                }
            }

            return $"{B}B {R}R";
        }

        private string TestSequence(Match.Symbols[] sequence)
        {
            // if am i arriving here and the _codifiedSequence is still setted to null, it mean that 'm trying to make an invalid move
            if (_codifiedSequence == null) throw new InvalidMove();
            byte B = 0, R = 0;
            for (byte i = 0; i < _codifiedSequence.Length; i++)
            {
                if (_codifiedSequence[i] == sequence[i]) B++;
                else
                {
                    for (byte j = 0; j < sequence.Length; j++)
                    {
                        if (_codifiedSequence[i] == sequence[j])
                        {
                            R++;
                            break;
                        }
                    }
                }
            }

            return $"{B}B {R}R";
        }

        public Match.Symbols[] GetNextSequence()
        {
            // if the sequences are null, it mean i'm tring to make an invalid Move
            if (_allPossibleSequenceCopy == null) throw new NullReferenceException();
            Match.Symbols[][] allSequences = _allPossibleSequenceCopy.ToArray();
            int randomSequence = new Random(DateTime.Now.Millisecond).Next(0, allSequences.Length);
            _codifiedSequence = allSequences[randomSequence];
            return _codifiedSequence;
        }

        public string MakeMoveAsCrypter(Match.Symbols[] sequence)
        {
            if (_IArole != Role.CRYPTER) throw new Exception();
            return TestSequence(sequence);
        }

        public Match.Symbols[] MakeMoveAsDecrypter(string response)
        {
            if (_IArole != Role.DECRYPTER) throw new Exception();

            // if the IA tried to guess at least 1 time i'll prune the sequence
            if (_codifiedSequence != null) PruneSequence(response);
            return GetNextSequence();
        }

        public Match.Symbols[] TakeSequence()
        {
            if (_IArole == Role.CRYPTER && _codifiedSequence != null) return _codifiedSequence;
            else throw new Exception();
        }

        public IAPlayer(Role role)
        {
            _IArole = role;
            GenerateAllPossibleSequence();
            _codifiedSequence = role == Role.CRYPTER ? GenereteCodifiedSequence() : null;
            _allPossibleSequenceCopy = MakeCopy(); 
        }

        private HashSet<Match.Symbols[]>? MakeCopy()
        {
            if(_allPossibileCodifiedSequence == null) return null;
            HashSet<Match.Symbols[]> allPossibleSequenceCopy = new(); 
            foreach(Match.Symbols[] symbols in _allPossibileCodifiedSequence){
                Match.Symbols[] symbolsCopy = new Match.Symbols[symbols.Length];
                for(byte i = 0; i < symbols.Length; i++){
                    symbolsCopy[i] = symbols[i];
                }
                allPossibleSequenceCopy.Add(symbolsCopy);
            }

            return allPossibleSequenceCopy;
        }
    }
}
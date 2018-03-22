using System;
using System.Collections.Generic;
using System.Linq;

namespace PockerGame
{
    class Program
    {
        static void Main(string[] args)
        {
            var cardsInput = "3H 6H KH 2H 4H 2D 7H 7H KS JH";
            var cards = cardsInput.Split(' ');
            var leftHand = cards.Take(5).Select(x => new Card(x)).ToArray();
            var rightHand = cards.Skip(5).Take(5).Select(x => new Card(x)).ToArray();

            List<IMatcher> matchers = new List<IMatcher>(10)
            {
                new RoyalFlushMatcher(),
                new StraightFlushMatcher(),
                new FourOfAKindMatcher(),
                new FullHouseMatcher(),
                new FlushMatcher(),
                new StraightMatcher(),
                new ThreeCardsMatcher(),
                new TwoPairsMatcher(),
                new OnePairMatcher(),
                new HighCardMatcher()
            };

            var leftStrengthMatcher = matchers.First(m => m.Match(leftHand));
            var rightStrengthMatcher = matchers.First(m => m.Match(rightHand));

            var matcherComparer = new MatcheComparer();
            int isLeftWinner = matcherComparer.Compare(leftStrengthMatcher, rightStrengthMatcher);

            if (isLeftWinner > 0)
            {
                Console.WriteLine("Righ hand is the winner");
            }
            else if (isLeftWinner < 0)
            {
                Console.WriteLine("Righ hand is the winner");
            }
            else
            {
                Console.WriteLine("draw");
            }
        }
    }


    public class Card
    {
        public char Color { get; set; }
        public int Number { get; set; }

        public Card(string stringCard)
        {
            Color = stringCard[1];

            var sigh = stringCard[0];

            if (sigh == 'T') Number = 10;
            if (sigh == 'J') Number = 11;
            if (sigh == 'Q') Number = 12;
            if (sigh == 'K') Number = 13;
            if (sigh == 'A') Number = 14;

            if (Number == 0)
            {
                Number = int.Parse(sigh.ToString());
            }
        }
    }

    public class MatcheComparer : IComparer<IMatcher>
    {
        public int Compare(IMatcher x, IMatcher y)
        {
            if (x.Strength > y.Strength) return 1;
            if (x.Strength < y.Strength) return -1;

            var handA = x.SortedCards;
            var handB = y.SortedCards;

            for (int i = 0; i < handA.Length; i++)
            {
                if (handA[i].Number > handB[i].Number) return 1;
                if (handA[i].Number < handB[i].Number) return -1;
            }

            return 0;
        }
    }

    public interface IMatcher
    {
        Card[] SortedCards { get; }
        int Strength { get; }
        bool Match(Card[] cards);
    }

    public abstract class BaseMatcher : IMatcher
    {
        protected Card[] _sortedCards;

        public Card[] SortedCards
        {
            get { return _sortedCards; }
        }

        public abstract int Strength { get; }
        public abstract bool Match(Card[] cards);
    }

    class HighCardMatcher : BaseMatcher
    {
        public override int Strength { get; } = 1;

        public override bool Match(Card[] cards)
        {
            _sortedCards = cards.OrderByDescending(x => x.Number).ToArray();
            return true;
        }
    }

    class OnePairMatcher : BaseMatcher
    {
        public override int Strength { get; } = 2;
        public override bool Match(Card[] cards)
        {
            var groupedCards = cards.GroupBy(x => x.Number);
            var isMatched = groupedCards.Count() < 5;
            if (isMatched)
            {
                var pair = groupedCards.Where(g => g.Count() > 1).SelectMany(g => g.ToArray()).ToList();
                var remaining = groupedCards.Where(g => g.Count() == 1).SelectMany(g => g.ToArray()).OrderByDescending(x => x.Number).ToList();
                pair.AddRange(remaining);
                _sortedCards = pair.ToArray();
            }

            return isMatched;
        }
    }

    class TwoPairsMatcher : BaseMatcher
    {
        public override int Strength { get; } = 3;

        public override bool Match(Card[] cards)
        {
            var groupedCards = cards.GroupBy(x => x.Number);
            var isMatched = groupedCards.Count() < 4;
            if (isMatched)
            {
                var pair = groupedCards.Where(g => g.Count() > 1).SelectMany(g => g.ToArray()).OrderByDescending(x => x.Number).ToList();
                var remaining = groupedCards.Where(g => g.Count() == 1).SelectMany(g => g.ToArray()).OrderByDescending(x => x.Number).ToList();
                pair.AddRange(remaining);
                _sortedCards = pair.ToArray();
            }

            return isMatched;
        }
    }

    class ThreeCardsMatcher : BaseMatcher
    {
        public override int Strength { get; } = 4;

        public override bool Match(Card[] cards)
        {
            var groupedCards = cards.GroupBy(x => x.Number);
            var isMatched = groupedCards.Any(g => g.Count() == 3);
            if (isMatched)
            {
                var pair = groupedCards.Where(g => g.Count() == 3).SelectMany(g => g.ToArray()).ToList();
                var remaining = groupedCards.Where(g => g.Count() == 1).SelectMany(g => g.ToArray()).OrderByDescending(x => x.Number).ToList();
                pair.AddRange(remaining);
                _sortedCards = pair.ToArray();
            }

            return isMatched;
        }
    }

    class StraightMatcher : BaseMatcher
    {
        public override int Strength { get; } = 5;
        public override bool Match(Card[] cards)
        {
            var isMatched = true;

            _sortedCards = cards.OrderByDescending(c => c.Number).ToArray();

            var previousCard = _sortedCards[0];

            for (int i = 1; i < _sortedCards.Length - 1; i++)
            {
                var currentCad = _sortedCards[i];
                if (previousCard.Number != currentCad.Number - 1)
                {
                    isMatched = false;
                    break;
                };
                previousCard = _sortedCards[i];
            }

            return isMatched;
        }
    }

    class FlushMatcher : BaseMatcher
    {
        public override int Strength { get; } = 6;
        public override bool Match(Card[] cards)
        {
            var isMatched = cards.GroupBy(x => x.Color).Count() == 1;

            if (isMatched)
            {
                _sortedCards = cards.OrderByDescending(c => c.Number).ToArray();
            }

            return isMatched;
        }
    }

    class FullHouseMatcher : BaseMatcher
    {
        public override int Strength { get; } = 7;
        public override bool Match(Card[] cards)
        {
            var isMatched = false;

            var numberGroup = cards.GroupBy(x => x.Number);
            if (numberGroup.Count() == 2)
            {
                if (!(numberGroup.Any(g => g.Count() == 1))) isMatched = true;
            }

            if (isMatched)
            {
                _sortedCards = cards.OrderByDescending(c => c.Number).ToArray();
            }

            return isMatched;
        }
    }

    class FourOfAKindMatcher : BaseMatcher
    {
        public override int Strength { get; } = 8;
        public override bool Match(Card[] cards)
        {
            var isMatched = false;

            var groupedCards = cards.GroupBy(x => x.Number);
            isMatched = groupedCards.Any(g => g.Count() == 4);

            if (isMatched)
            {
                var pair = groupedCards.Where(g => g.Count() == 4).SelectMany(g => g.ToArray()).ToList();
                var remaining = groupedCards.Where(g => g.Count() == 1).SelectMany(g => g.ToArray()).OrderByDescending(x => x.Number).ToList();
                pair.AddRange(remaining);
                _sortedCards = pair.ToArray();
            }

            return isMatched;
        }
    }

    class StraightFlushMatcher : BaseMatcher
    {
        private StraightMatcher _straightMatcher = new StraightMatcher();

        public override int Strength { get; } = 10;
        public override bool Match(Card[] cards)
        {
            var isMatched = cards.GroupBy(x => x.Color).Count() == 1;

            if (isMatched)
            {
                this._sortedCards = _straightMatcher.SortedCards;
            }

            return isMatched;
        }
    }

    class RoyalFlushMatcher : BaseMatcher
    {
        private StraightMatcher _straightMatcher = new StraightMatcher();
        private StraightFlushMatcher _straightFlushMatcher = new StraightFlushMatcher();

        public override int Strength { get; } = 10;
        public override bool Match(Card[] cards)
        {
            var isMatched = _straightFlushMatcher.Match(cards) && _straightMatcher.Match(cards);

            if (isMatched)
            {
                _sortedCards = _straightMatcher.SortedCards;
            }

            return isMatched;
        }
    }
}

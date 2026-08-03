using System;
using System.Collections.Generic;
using System.Text;

namespace WatchListScreening.Domain.Enums;

public enum MatchType
{
    Exact = 1,
    Contains = 2,
    Fuzzy = 3,
    Phonetic = 4
}
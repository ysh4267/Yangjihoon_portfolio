using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICardDBData
{
    public int temporaryID { get; set; }
    public bool isFixedInDeck { get; set; }
    public bool isInDeck { get; set; }
}

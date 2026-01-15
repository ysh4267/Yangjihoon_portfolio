using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IRenderableData : IIndexableDTO {
    enum ItemType { card, skill, equipment, starterPack, portrait, buff }
    public ItemType datatype { get; set; }
    public Illustration Illust { get; set; }
    public string Description { get; set; }
    public string Name { get; set; }

}

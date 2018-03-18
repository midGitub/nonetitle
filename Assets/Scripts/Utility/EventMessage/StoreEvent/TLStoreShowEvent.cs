using System.Collections;
using System.Collections.Generic;
using CitrusFramework;

public class TLStoreShowEvent : CitrusGameEvent {

    public IAPCatalogData item;

    public TLStoreShowEvent(IAPCatalogData item){
        this.item = item;
    }
}

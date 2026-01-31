using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[CreateAssetMenu(menuName = "Custom/Item Behaviors/Document Behavior")]
public class DocumentBehavior : ItemBehavior
{
    public static event Action<DocumentBehavior> OnDocumentUsed;

    public AssetReferenceSprite documentImage;

    public override bool CanUse(ItemData item)
    {
        return documentImage != null && documentImage.RuntimeKeyIsValid();
    }

    public override void OnUse(ItemData item)
    {
        if (documentImage != null && documentImage.RuntimeKeyIsValid())
        {
            OnDocumentUsed.Invoke(this);

            if (documentImage.IsValid() && documentImage.OperationHandle.IsValid())
            {
                var sprite = documentImage.OperationHandle.Convert<Sprite>().Result;
                Core.UI.Inspect.ShowDocument(sprite);
            }
            else
            {
                var loadOp = documentImage.LoadAssetAsync<Sprite>();
                loadOp.Completed += OnSpriteLoaded;
            }
        }
    }

    private void OnSpriteLoaded(AsyncOperationHandle<Sprite> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            Core.UI.Inspect.ShowDocument(handle.Result);
        }
    }
}
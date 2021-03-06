// Copyright 1998-2016 Epic Games, Inc. All Rights Reserved.

/**
 * Abstract base class of animation composite base
 * This contains Composite Section data and some necessary interface to make this work
 */

#pragma once
#include "EditorAnimBaseObj.generated.h"

class UAnimSequenceBase;

DECLARE_DELEGATE_TwoParams( FOnAnimObjectChange, class UObject*, bool)


UCLASS(abstract, MinimalAPI)
class UEditorAnimBaseObj: public UObject
{
	GENERATED_UCLASS_BODY()
public:
	

	virtual void InitFromAnim(UAnimSequenceBase* AnimObjectIn, FOnAnimObjectChange OnChange);
	virtual bool ApplyChangesToMontage();

	UAnimSequenceBase* AnimObject;
	FOnAnimObjectChange OnChange;

	virtual void PreEditChange( class FEditPropertyChain& PropertyAboutToChange ) override;

	virtual void PostEditChangeProperty(FPropertyChangedEvent& PropertyChangedEvent) override;

	virtual bool PropertyChangeRequiresRebuild(FPropertyChangedEvent& PropertyChangedEvent) { return true;}

	//void NotifyUser();
};

// Copyright 1998-2016 Epic Games, Inc. All Rights Reserved.

#pragma once


class UMovieSceneSection;
class FSequencerDisplayNode;


enum class ESelectionPreviewState
{
	Undefined,
	Selected,
	NotSelected
};


/**
 * Manages the selection of keys, sections, and outliner nodes for the sequencer.
 */
class FSequencerSelectionPreview
{
public:

	/** Access the defined key states */
	const TMap<FSequencerSelectedKey, ESelectionPreviewState>& GetDefinedKeyStates() const { return DefinedKeyStates; }

	/** Access the defined section states */
	const TMap<TWeakObjectPtr<UMovieSceneSection>, ESelectionPreviewState>& GetDefinedSectionStates() const { return DefinedSectionStates; }

	/** Adds a key to the selection */
	void SetSelectionState(FSequencerSelectedKey Key, ESelectionPreviewState InState);

	/** Adds a section to the selection */
	void SetSelectionState(UMovieSceneSection* Section, ESelectionPreviewState InState);

	/** Returns the selection state for the the specified key. */
	ESelectionPreviewState GetSelectionState(FSequencerSelectedKey Key) const;

	/** Returns the selection state for the the specified section. */
	ESelectionPreviewState GetSelectionState(UMovieSceneSection* Section) const;

	/** Empties all selections. */
	void Empty();

	/** Empties the key selection. */
	void EmptyDefinedKeyStates();

	/** Empties the section selection. */
	void EmptyDefinedSectionStates();

private:

	TMap<FSequencerSelectedKey, ESelectionPreviewState> DefinedKeyStates;
	TMap<TWeakObjectPtr<UMovieSceneSection>, ESelectionPreviewState> DefinedSectionStates;
};

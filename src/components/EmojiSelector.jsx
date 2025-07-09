import React from 'react';

const EMOJIS = [
  { emoji: '💩', value: 'poop' },
  { emoji: '😃', value: 'smiley' },
  { emoji: '🔥', value: 'fire' },
  { emoji: '❤️', value: 'heart' }
];

export default function EmojiSelector({ selectedEmoji, onEmojiSelect }) {
  const handleEmojiClick = (emojiData) => {
    if (selectedEmoji === emojiData.value) {
      // Unselect if clicking the same emoji
      onEmojiSelect(null);
    } else {
      // Select new emoji
      onEmojiSelect(emojiData.value);
    }
  };

  return (
    <div className="bg-white rounded-lg shadow-md p-4">
      <h3 className="text-lg font-semibold mb-3 text-gray-800">Emoji Reaction</h3>
      <div className="flex justify-center gap-4">
        {EMOJIS.map((emojiData) => (
          <button
            key={emojiData.value}
            onClick={() => handleEmojiClick(emojiData)}
            className={`
              text-3xl p-3 rounded-full transition-all duration-200 transform hover:scale-110
              ${selectedEmoji === emojiData.value 
                ? 'bg-blue-100 ring-2 ring-blue-400 shadow-lg' 
                : 'bg-gray-50 hover:bg-gray-100'
              }
            `}
            title={`Select ${emojiData.value} emoji`}
          >
            {emojiData.emoji}
          </button>
        ))}
      </div>
      {selectedEmoji && (
        <p className="text-center text-sm text-gray-600 mt-2">
          Selected: {selectedEmoji}
        </p>
      )}
    </div>
  );
}
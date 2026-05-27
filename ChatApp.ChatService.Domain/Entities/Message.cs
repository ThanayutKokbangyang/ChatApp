namespace ChatApp.ChatService.Domain.Entities
{
    public class Message
    {
        public Guid Id { get; private set; }
        public Guid RoomId { get; private set; }
        public Guid SenderId { get; private set; }
        public string Content { get; private set; } = string.Empty;
        public string MessageType { get; private set; } = "text";
        public DateTime SentAt { get; private set; }
        public bool IsDeleted { get; private set; }

        private const int MaxTextContentLength = 4000;
        private const int MaxMediaContentLength = 1000;

        private Message() { }

        public static Message Create(
            Guid roomId,
            Guid senderId,
            string content,
            string messageType = "text")
        {
            if (roomId == Guid.Empty)
                throw new Exceptions.DomainException("RoomId is required");

            if (senderId == Guid.Empty)
                throw new Exceptions.DomainException("SenderId is required");

            if (string.IsNullOrWhiteSpace(content))
                throw new Exceptions.DomainException("Message content cannot be empty");

            if (messageType != "text" &&
                messageType != "image" &&
                messageType != "sticker")
            {
                throw new Exceptions.DomainException("Invalid message type");
            }

            var maxLength = messageType == "text"
                ? MaxTextContentLength
                : MaxMediaContentLength;

            if (content.Length > maxLength)
                throw new Exceptions.DomainException(
                    $"Message cannot exceed {maxLength} characters");

            return new Message
            {
                Id = Guid.NewGuid(),
                RoomId = roomId,
                SenderId = senderId,
                Content = content,
                MessageType = messageType,
                SentAt = DateTime.UtcNow,
                IsDeleted = false
            };
        }

        public void SoftDelete()
        {
            if (IsDeleted)
                throw new Exceptions.DomainException("Message is already deleted");

            IsDeleted = true;
        }
    }
}
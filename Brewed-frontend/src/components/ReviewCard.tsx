import { Card, Group, Text, Rating, ActionIcon } from "@mantine/core";
import { IconTrash } from "@tabler/icons-react";
import { IReview } from "../interfaces/IReview";

interface ReviewCardProps {
  review: IReview;
  canDelete?: boolean;
  onDelete?: (reviewId: number) => void;
}

const ReviewCard = ({ review, canDelete, onDelete }: ReviewCardProps) => {
  return (
    <Card
      withBorder
      p="md"
      radius="lg"
      style={{
        borderColor: 'rgba(139, 69, 19, 0.1)',
        transition: 'box-shadow 0.2s ease',
      }}
      onMouseEnter={(e) => { e.currentTarget.style.boxShadow = '0 4px 12px rgba(139, 69, 19, 0.06)'; }}
      onMouseLeave={(e) => { e.currentTarget.style.boxShadow = ''; }}
    >
      <Group justify="space-between" mb="xs">
        <div>
          <Text fw={600} size="sm">{review.userName}</Text>
          <Text size="xs" c="dimmed">
            {new Date(review.createdAt).toLocaleDateString()}
          </Text>
        </div>
        <Group gap="xs">
          <Rating value={review.rating} readOnly size="sm" color="brown" />
          {canDelete && onDelete && (
            <ActionIcon color="red" variant="subtle" radius="md" onClick={() => onDelete(review.id)}>
              <IconTrash size={16} />
            </ActionIcon>
          )}
        </Group>
      </Group>

      {review.title && (
        <Text fw={600} mb="xs" style={{ color: '#3d3d3d' }}>
          {review.title}
        </Text>
      )}

      <Text size="sm" style={{ color: '#5c5c5c', lineHeight: 1.6 }}>{review.comment}</Text>
    </Card>
  );
};

export default ReviewCard;
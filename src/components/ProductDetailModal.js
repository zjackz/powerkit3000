
import React from 'react';
import { Modal, Row, Col, Typography, Statistic, Tag } from 'antd';
import { Line } from '@ant-design/charts';

const { Title, Text } = Typography;

// --- Mock Chart Data ---
const mockHistoryData = [
  { date: '2025-07-07', value: 15, type: 'BSR' },
  { date: '2025-07-08', value: 12, type: 'BSR' },
  { date: '2025-07-09', value: 10, type: 'BSR' },
  { date: '2025-07-10', value: 8, type: 'BSR' },
  { date: '2025-07-11', value: 5, type: 'BSR' },
  { date: '2025-07-12', value: 7, type: 'BSR' },
  { date: '2025-07-13', value: 6, type: 'BSR' },
  { date: '2025-07-07', value: 149.99, type: 'Price' },
  { date: '2025-07-08', value: 149.99, type: 'Price' },
  { date: '2025-07-09', value: 145.00, type: 'Price' },
  { date: '2025-07-10', value: 145.00, type: 'Price' },
  { date: '2025-07-11', value: 139.99, type: 'Price' },
  { date: '2025-07-12', value: 139.99, type: 'Price' },
  { date: '2025-07-13', value: 149.99, type: 'Price' },
];

const ProductDetailModal = ({ visible, onCancel, product }) => {
  if (!product) return null;

  const chartConfig = {
    data: mockHistoryData,
    xField: 'date',
    yField: 'value',
    seriesField: 'type',
    yAxis: {
      BSR: { min: 0, title: { text: 'BSR' } },
      Price: { min: 0, title: { text: 'Price ($' } },
    },
    geometryOptions: [
        {
            geometry: 'line',
            color: ['#5B8FF9', '#5AD8A6'],
        },
    ],
    tooltip: { showMarkers: true },
    legend: { position: 'top-right' },
  };

  return (
    <Modal
      visible={visible}
      onCancel={onCancel}
      footer={null}
      width={800}
      title="Product Details"
    >
      <Row gutter={[24, 24]}>
        <Col span={24}>
          <Title level={4}>{product.title}</Title>
          <a href={`https://www.amazon.com/dp/${product.asin}`} target="_blank" rel="noopener noreferrer">View on Amazon</a>
        </Col>
        <Col span={8}>
            <Statistic title="Current BSR" value={product.bsr} />
        </Col>
        <Col span={8}>
            <Statistic title="Price" value={product.price} prefix="$" />
        </Col>
        <Col span={8}>
            <Statistic title="Reviews" value={product.reviews} />
        </Col>
      </Row>
      <Row style={{ marginTop: '24px' }}>
        <Col span={24}>
            <Title level={5}>30-Day History</Title>
            <Line {...chartConfig} />
        </Col>
      </Row>
    </Modal>
  );
};

export default ProductDetailModal;

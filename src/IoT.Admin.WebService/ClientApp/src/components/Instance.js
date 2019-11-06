import React from 'react';
import { connect } from 'react-redux';

const Instance = props => (
  <div>
    <form>
  <label>
    Name:
    <input type="text" name="name" />
  </label>
  <input type="submit" value="Submit" />
</form>
  </div>
);

export default connect()(Instance);

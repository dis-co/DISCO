import React from 'react';
import Form from 'muicss/lib/react/form';
import Input from 'muicss/lib/react/input';
import Button from 'muicss/lib/react/button';
import Panel from 'muicss/lib/react/panel';

const STATUS = {
  AUTHORIZED: "Authorized",
  WAITING: "Waiting",
  UNAUTHORIZED: "Unauthorized"
}

export default class LoginDialog extends React.Component {
  constructor(props) {
      super(props);
      this.state = { active: false, username: "", password: "" };
  }

  handleSubmit(username, password) {
    this.props.login(username, password);
    this.setState({status: STATUS.WAITING});
  };

  componentWillReceiveProps(nextProps) {
    if (nextProps.session) {
      let status = nextProps.session.Status.StatusType.ToString();
      switch (status) {
        case STATUS.AUTHORIZED:
          this.setState({ status })
          break;
        case STATUS.UNAUTHORIZED:
          if (this.state.status === STATUS.WAITING) {
            this.setState({ status })
          }
          break;
      }
    }
  }

  getMessage() {
    switch (this.state.status) {
      case STATUS.AUTHORIZED:
        return "Login succeeded!";
      case STATUS.WAITING:
        return "Waiting for response";
      case STATUS.UNAUTHORIZED:
        return "Input data is not correct";
    }
  }

  render() {
    return (
      <Panel style={{width: 500, margin: "auto"}}>
        <Form>
          <legend>Login</legend>
          <Input name="username" label="Username" floatingLabel={true} required={true} />
          <Input name="password" label="Password" type="password" floatingLabel={true} required={true} />
          <Button variant="raised"
            disabled={this.state.status == STATUS.WAITING}
            onClick={ev => {
              ev.preventDefault();
              var form = ev.target.parentNode;
              this.handleSubmit(form.username.value, form.password.value);
            }}>
            Submit
          </Button>
          <p>{this.getMessage()}</p>
        </Form>
      </Panel>
    );
  }
}

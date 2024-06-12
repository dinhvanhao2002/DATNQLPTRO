// import { FormGroup } from '@angular/forms';
// export function ConfirmPasswordValidator(controlName : string, matchControlName: string)
// {
//     return (formGroup: FormGroup)=> {
//         // const khai báo biến không thể thay đổi
//         // let có thể khai báo biến có thể thay đổi giá tị

//         const passwordControl = formGroup.controls[controlName];
//         const confirmPasswordControl = formGroup.controls[matchControlName];
//         if(confirmPasswordControl.errors && ! confirmPasswordControl.errors['confirmPasswordValidator'])
//         {
//             return;
//         }
//         if(passwordControl.value !== confirmPasswordControl.value)
//         {
//             confirmPasswordControl.setErrors({ConfirmPasswordValidator : true});
//         }
//         else{
//             confirmPasswordControl.setErrors(null);
//         }
//     }
// }
